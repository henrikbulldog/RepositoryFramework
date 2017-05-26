using System.Collections.Generic;
using System.Linq;
using Xunit;
using RepositoryFramework.Test.Models;
using System;
using RepositoryFramework.Test.Filters;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Threading.Tasks;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.Dapper.Test
{
  public class DapperRepositoryAsyncTest
  {
    [Fact]
    public async Task GetByIdAsync()
    {
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection, "RepositoryTest_Create_And_GetById");

        // Arrange
        var categoryRepository = CreateCategoryRepository(connection);
        var category = new Category
        {
          Name = Guid.NewGuid().ToString(),
          Description = Guid.NewGuid().ToString()
        };

        // Act
        categoryRepository.Create(category);

        // Assert
        var result = await categoryRepository.GetByIdAsync(category.Id.Value);
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
        Assert.Equal(category.Description, result.Description);
      }
    }

    [Fact]
    public async Task CreateManyAsync()
    {
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection, "RepositoryTest_Delete");

        // Arrange
        var totalCategories = 100;
        var createThese = new List<Category>();
        var categoryRepository = CreateCategoryRepository(connection);
        for (int i = 0; i < totalCategories; i++)
        {
          var category = new Category
          {
            Name = i.ToString(),
            Description = i.ToString()
          };
          createThese.Add(category);
        }

        // Act
        await categoryRepository.CreateManyAsync(createThese);

        // Assert
        var result = await categoryRepository.FindAsync();
        Assert.NotNull(result);
        Assert.Equal(totalCategories, result.Count());
      }
    }

    [Fact]
    public async Task UpdateAsync()
    {
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection, "RepositoryTest_UpdateAsync");

        // Arrange
        var categoryRepository = CreateCategoryRepository(connection);
        var category = new Category
        {
          Name = Guid.NewGuid().ToString(),
          Description = Guid.NewGuid().ToString()
        };

        // Act
        categoryRepository.Create(category);
        category.Name = "New Name";
        category.Description = "New Description";
        await categoryRepository.UpdateAsync(category);

        // Assert
        var result = categoryRepository.GetById(category.Id.Value);
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
        Assert.Equal(category.Description, result.Description);
      }
    }

    [Fact]
    public async Task DeleteAsync()
    {
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection, "RepositoryTest_Delete");

        // Arrange
        var categoryRepository = CreateCategoryRepository(connection);
        var category = new Category
        {
          Name = Guid.NewGuid().ToString(),
          Description = Guid.NewGuid().ToString()
        };

        // Act
        categoryRepository.Create(category);
        var result = categoryRepository.GetById(category.Id.Value);
        Assert.NotNull(result);
        await categoryRepository.DeleteAsync(category);

        // Assert
        result = categoryRepository.GetById(category.Id.Value);
        Assert.Null(result);
      }
    }

    [Fact]
    public async Task DeleteManyAsync()
    {
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection, "RepositoryTest_Delete");

        // Arrange
        var totalCategories = 100;
        var deleteThese = new List<Category>();
        var categoryRepository = CreateCategoryRepository(connection);
        for (int i = 0; i < totalCategories; i++)
        {
          var category = new Category
          {
            Name = i.ToString(),
            Description = i.ToString()
          };
          categoryRepository.Create(category);
          if (i % 2 == 0)
          {
            deleteThese.Add(category);
          }
        }

        // Act
        await categoryRepository.DeleteManyAsync(deleteThese);

        // Assert
        var result = await categoryRepository.FindAsync();
        Assert.NotNull(result);
        Assert.Equal(totalCategories / 2, result.Count());
      }
    }

    [Fact]
    public async Task FindAsync()
    {
      var rows = 100;
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection, "RepositoryTest_Find");

        // Arrange
        var categories = new List<Category>();
        for (int i = 0; i < rows; i++)
        {
          var category = new Category
          {
            Name = i.ToString(),
            Description = i.ToString()
          };
          categories.Add(category);
        }
        var categoryRepository = CreateCategoryRepository(connection);
        await categoryRepository.CreateManyAsync(categories);

        // Act
        var result = await categoryRepository.FindAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rows, result.Count());
      }
    }

    [Fact]
    public async Task Combine_Page_Sort_FindAsync()
    {
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection, "RepositoryTest_Find");

        // Arrange
        var categories = new List<Category>();
        for (int i = 0; i < 100; i++)
        {
          var category = new Category
          {
            Name = i.ToString(),
            Description = i.ToString()
          };
          categories.Add(category);
        }
        var categoryRepository = CreateCategoryRepository(connection);
        await categoryRepository.CreateManyAsync(categories);

        // Act
        var result = await categoryRepository
          .Page(2, 40)
          .SortBy(c => c.Id)
          .FindAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(40, result.Count());
      }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task SortBy(bool descendingOrder, bool useExpression)
    {
      // Create new empty database
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection, "RepositoryTest_SortBy");

        // Arrange
        var categories = new List<Category>();
        for (int i = 0; i < 100; i++)
        {
          var category = new Category
          {
            Name = $"{i % 10}",
            Description = i.ToString()
          };
          categories.Add(category);
        }
        ISortableRepository<Category> categoryRepository = CreateCategoryRepository(connection);
        await categoryRepository.CreateManyAsync(categories);

        // Act
        if (descendingOrder)
        {
          if (useExpression)
          {
            categoryRepository.SortByDescending("Name");
          }
          else
          {
            categoryRepository.SortByDescending(p => p.Name);
          }
        }
        else
        if (useExpression)
        {
          categoryRepository.SortBy("Name");
        }
        else
        {
          categoryRepository.SortBy(p => p.Name);
        }

        var result = (await categoryRepository.FindAsync()).ToList();

        // Assert
        var sortedCategories = descendingOrder
          ? result.OrderByDescending(p => p.Name)
          : result.OrderBy(p => p.Name);
        Assert.NotNull(result);
        Assert.Equal(result, sortedCategories);

        // Act
        result = (await categoryRepository.ClearSorting().FindAsync()).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(result, sortedCategories);
      }
    }

    [Theory]
    [InlineData(1, 40, 100, 40)]
    [InlineData(2, 40, 100, 40)]
    [InlineData(3, 40, 100, 20)]
    [InlineData(4, 40, 100, 0)]
    public async Task PageAsync(int page, int pageSize, int totalRows, int expectedRows)
    {
      // Create new empty database
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection, "RepositoryTest_Page");

        // Arrange
        var categories = new List<Category>();
        for (int i = 0; i < totalRows; i++)
        {
          var category = new Category
          {
            Name = $"{i % 10}",
            Description = i.ToString()
          };
          categories.Add(category);
        }
        IPageableRepository<Category> categoryRepository = CreateCategoryRepository(connection);
        await categoryRepository.CreateManyAsync(categories);

        // Act
        var pageItems = await categoryRepository
          .Page(page, pageSize)
          .FindAsync();

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(expectedRows, pageItems.Count());

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(expectedRows, pageItems.Count());

        // Act
        pageItems = await categoryRepository
          .ClearPaging()
          .FindAsync();

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(totalRows, pageItems.Count());
      }
    }

    private IDbConnection CreateConnection()
    {
      return new SqliteConnection("Data Source=:memory:");
    }

    private void InitializeDatabase(IDbConnection connection, string database)
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
NullField NVARCHAR(100) DEFAULT NULL,
DateTimeField DATETIME,
Description NVARCHAR(100)
)
";

      command.ExecuteNonQuery();
    }

    private IDapperRepository<Category> CreateCategoryRepository(IDbConnection connection)
    {
      return new DapperRepository<Category>(
        connection,
        "SELECT last_insert_rowid()",
        "LIMIT {PageSize} OFFSET ({PageNumber} - 1) * {PageSize}");
    }
  }
}
