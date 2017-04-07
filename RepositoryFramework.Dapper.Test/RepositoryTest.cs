using System.Collections.Generic;
using System.Linq;
using Xunit;
using RepositoryFramework.Test.Models;
using System;
using RepositoryFramework.Test.Filters;
using Microsoft.Data.Sqlite;
using System.Data;

namespace RepositoryFramework.Dapper.Test
{
  public class RepositoryTest
  {
    protected virtual IDbConnection CreateConnection()
    {
      return new SqliteConnection("Data Source=:memory:");
    }

    protected virtual void InitializeDatabase(IDbConnection connection)
    {
      if (connection.State != ConnectionState.Open)
      {
        connection.Open();
      }

      var command = connection.CreateCommand();
      command.CommandText = @"
CREATE TABLE Category (
Id INTEGER PRIMARY KEY,
Name NVARCHAR(100),
Description NVARCHAR(100)
)";

      command.ExecuteNonQuery();
    }

    protected virtual Repository<Category, TFilter> CreateCategoryRepository<TFilter>(IDbConnection connection)
    {
      return new Repository<Category, TFilter>(
          connection,
          "SELECT last_insert_rowid()",
          (c) => c.Id);
    }

    [Fact]
    public void Create_And_GetById()
    {
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection);

        // Arrange
        var categoryRepository = CreateCategoryRepository<IdFilter>(connection);
        var category = new Category
        {
          Name = Guid.NewGuid().ToString(),
          Description = Guid.NewGuid().ToString()
        };

        // Act
        categoryRepository.Create(category);

        // Assert
        var result = categoryRepository.GetById(category.Id.Value);
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
        Assert.Equal(category.Description, result.Description);
      }
    }

    [Fact]
    public void Update()
    {
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection);

        // Arrange
        var categoryRepository = CreateCategoryRepository<IdFilter>(connection);
        var category = new Category
        {
          Name = Guid.NewGuid().ToString(),
          Description = Guid.NewGuid().ToString()
        };

        // Act
        categoryRepository.Create(category);
        category.Name = "New Name";
        category.Description = "New Description";
        categoryRepository.Update(category);

        // Assert
        var result = categoryRepository.GetById(category.Id.Value);
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
        Assert.Equal(category.Description, result.Description);
      }
    }

    [Fact]
    public void Delete()
    {
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection);

        // Arrange
        var categoryRepository = CreateCategoryRepository<IdFilter>(connection);
        var category = new Category
        {
          Name = Guid.NewGuid().ToString(),
          Description = Guid.NewGuid().ToString()
        };

        // Act
        categoryRepository.Create(category);
        var result = categoryRepository.GetById(category.Id.Value);
        Assert.NotNull(result);
        categoryRepository.Delete(category);

        // Assert
        result = categoryRepository.GetById(category.Id.Value);
        Assert.Null(result);
      }
    }

    [Theory]
    [InlineData(100)]
    public void Find(int categories)
    {
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection);

        // Arrange
        var categoryRepository = CreateCategoryRepository<IdFilter>(connection);
        for (int i = 0; i < categories; i++)
        {
          var category = new Category
          {
            Name = i.ToString(),
            Description = i.ToString()
          };
          categoryRepository.Create(category);
        }

        // Act
        var result = categoryRepository.Find();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categories, result.TotalCount);
      }
    }

    [Theory]
    [InlineData(100, "Name 1", null, 1)]
    [InlineData(100, "Name 1", "Description 1", 1)]
    [InlineData(100, null, "Description 1", 10)]
    public void FindFilter(int categories, string name, string description, int expectedRows)
    {
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection);

        // Arrange
        var categoryRepository = CreateCategoryRepository<CategoryFilter>(connection);
        for (int i = 0; i < categories; i++)
        {
          var category = new Category
          {
            Name = $"Name {i}",
            Description = $"Description {i % 10}"
          };
          categoryRepository.Create(category);
        }

        // Act
        var result = categoryRepository.Find(new CategoryFilter { Name = name, Description = description });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRows, result.TotalCount);
      }
    }
  }
}
