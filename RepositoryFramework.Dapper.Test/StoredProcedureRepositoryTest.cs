using System.Collections.Generic;
using System.Linq;
using Xunit;
using RepositoryFramework.Test.Models;
using System;
using RepositoryFramework.Test.Filters;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.Dapper.Test
{
  public class StoredProcedureRepositoryTest
  {
    [Theory]
    [InlineData(100, "Name", "Name 1", 1)]
    [InlineData(100, "Description", "Description 1", 10)]
    public void FindParameter(int categories, string parameterName, object parameterValue, int expectedRows)
    {
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection, "RepositoryTest_FindFilter");

        // Arrange
        IParameterizedRepository<Category> categoryRepository = CreateCategoryRepository(connection);
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
        var result = categoryRepository
          .SetParameter(parameterName, parameterValue)
          .Find();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRows, result.Count());
      }
    }

    [Fact]
    public void GetById()
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
        var result = categoryRepository.GetById(category.Id.Value);
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
        Assert.Equal(category.Description, result.Description);
      }
    }

    [Fact]
    public void CreateMany()
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
        categoryRepository.CreateMany(createThese);

        // Assert
        var result = categoryRepository.Find();
        Assert.NotNull(result);
        Assert.Equal(totalCategories, result.Count());
      }
    }

    [Fact]
    public void Update()
    {
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection, "RepositoryTest_Update");

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
        categoryRepository.Delete(category);

        // Assert
        result = categoryRepository.GetById(category.Id.Value);
        Assert.Null(result);
      }
    }

    [Fact]
    public void DeleteMany()
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
        categoryRepository.DeleteMany(deleteThese);

        // Assert
        var result = categoryRepository.Find();
        Assert.NotNull(result);
        Assert.Equal(totalCategories / 2, result.Count());
      }
    }

    [Theory]
    [InlineData(100)]
    public void Find(int categories)
    {
      using (var connection = CreateConnection())
      {
        InitializeDatabase(connection, "RepositoryTest_Find");

        // Arrange
        var categoryRepository = CreateCategoryRepository(connection);
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
        Assert.Equal(categories, result.Count());
      }
    }
    private IStoredProcedureDapperRepository<Category> CreateCategoryRepository(IDbConnection connection)
    {
      return new StoredProcedureDapperRepository<Category>(connection);
    }
    private IDbConnection CreateConnection()
    {
      return new SqlConnection(@"Server=(LocalDb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;");
    }

    private static void RunScript(IDbConnection connection, string script)
    {
      Regex regex = new Regex(@"\r{0,1}\nGO\r{0,1}\n");
      string[] commands = regex.Split(script);

      for (int i = 0; i < commands.Length; i++)
      {
        if (commands[i] != string.Empty)
        {
          using (var command = connection.CreateCommand())
          {
            command.CommandText = commands[i];
            command.ExecuteNonQuery();
          }
        }
      }
    }

    private void InitializeDatabase(IDbConnection connection, string database)
    {
      if (connection.State != ConnectionState.Open)
      {
        connection.Open();
      }

      var script = $@"
USE master
IF EXISTS(SELECT * FROM sys.databases WHERE NAME='{database}')
	DROP DATABASE [{database}]
GO

CREATE DATABASE [{database}]
GO

USE [{database}]
IF OBJECT_ID ('Category') IS NOT NULL 
  DROP TABLE Category
GO

CREATE TABLE Category (
  Id INTEGER IDENTITY,
  Name NVARCHAR(100),
  NullField NVARCHAR(100) DEFAULT NULL,
  DateTimeField DATETIME,
  Description NVARCHAR(100)
)
GO

IF OBJECT_ID ('CreateCategory') IS NOT NULL 
  DROP PROCEDURE CreateCategory
GO

CREATE PROCEDURE CreateCategory
  @Name NVARCHAR(100) = NULL,
  @NullField NVARCHAR(100) = NULL,
  @DateTimeField DATETIME = NULL,
  @Description NVARCHAR(100) = NULL
AS
BEGIN
  INSERT INTO Category (Name, NullField, DateTimeField, Description)
  VALUES(@Name, @NullField, @DateTimeField, @Description)

  SELECT @@IDENTITY
END
GO

IF OBJECT_ID ('GetCategory') IS NOT NULL 
  DROP PROCEDURE GetCategory
GO
  
CREATE PROCEDURE GetCategory
  @Id INTEGER
AS
BEGIN
  SELECT * 
  FROM Category
  WHERE Id = @Id
END
GO

IF OBJECT_ID ('FindCategory') IS NOT NULL 
  DROP PROCEDURE FindCategory
GO
  
CREATE PROCEDURE FindCategory
  @Name NVARCHAR(100) = NULL,
  @Description NVARCHAR(100) = NULL
AS
BEGIN
  SELECT * 
  FROM Category
  WHERE (Name = @Name OR @Name IS NULL)
    AND (Description = @Description OR @Description IS NULL)
END
GO

IF OBJECT_ID ('DeleteCategory') IS NOT NULL 
  DROP PROCEDURE DeleteCategory
GO
  
CREATE PROCEDURE DeleteCategory
  @Id INTEGER
AS
BEGIN
  DELETE 
  FROM Category
  WHERE Id = @Id
END
GO

IF OBJECT_ID ('UpdateCategory') IS NOT NULL 
  DROP PROCEDURE UpdateCategory
GO
  
CREATE PROCEDURE UpdateCategory
  @Id INTEGER,
  @Name NVARCHAR(100),
  @NullField NVARCHAR(100),
  @DateTimeField DATETIME,
  @Description NVARCHAR(100)
AS
BEGIN
  UPDATE Category
  SET Name = @Name,
    Description = @Description,
    NullField = @NullField,
    DateTimeField = @DateTimeField
  WHERE Id = @Id
END
";

      RunScript(connection, script);
    }

  }
}
