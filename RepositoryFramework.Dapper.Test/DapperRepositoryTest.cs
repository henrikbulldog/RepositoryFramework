using System.Collections.Generic;
using System.Linq;
using Xunit;
using RepositoryFramework.Test.Models;
using System;
using RepositoryFramework.Test.Filters;
using Microsoft.Data.Sqlite;
using System.Data;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.Dapper.Test
{
  public class DapperRepositoryTest
  {
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

    [Fact]
    public void Find()
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
        categoryRepository.CreateMany(categories);

        // Act
        var result = categoryRepository.Find();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rows, result.Count());
      }
    }

    [Fact]
    public void Combine_Page_Sort_Find()
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
        categoryRepository.CreateMany(categories);

        // Act
        var result = categoryRepository
          .Page(2, 40)
          .SortBy(c => c.Id)
          .Find();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(40, result.Count());
      }
    }


    [Fact]
    public void Combine_Page_Sort_FindSql()
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
        categoryRepository.CreateMany(categories);

        // Act
        var result = categoryRepository
          .Page(2, 40)
          .SortBy(c => c.Id)
          .Find("SELECT * FROM Category WHERE Id > @Id AND Description <> @Description",
          new Dictionary<string, object>
          {
            { "Description", "XXX" },
            { "Id", 50 }
          });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Count());
      }
    }

    [Fact]
    public void FindSql_No_Parameters()
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
        var categoryRepository = (DapperRepository<Category>)CreateCategoryRepository(connection);
        categoryRepository.CreateMany(categories);

        // Act
        var result = categoryRepository
          .Find("SELECT * FROM Category");

        // Assert
        Assert.Equal(100, result.Count());
      }
    }

    [Fact]
    public void FindSql_With_Parameters()
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
        var categoryRepository = (DapperRepository<Category>)CreateCategoryRepository(connection);
        categoryRepository.CreateMany(categories);

        // Act
        var result = categoryRepository
          .Find("SELECT * FROM Category WHERE Id > @Id AND Description <> @Description",
          new Dictionary<string, object>
          {
            { "Description", "XXX" },
            { "Id", 50 }
          });

        // Assert
        Assert.Equal(50, result.Count());
      }
    }

    [Fact]
    public void FindSql_With_Parameters_And_Pattern()
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
        var categoryRepository = (DapperRepository<Category>)CreateCategoryRepository(connection);
        categoryRepository.CreateMany(categories);

        // Act
        var result = categoryRepository
          .Find("SELECT * FROM Category WHERE Id > :Id AND Description <> :Description",
          new Dictionary<string, object>
          {
            { "Description", "XXX" },
            { "Id", 50 }
          },
          @":(\w+)");

        // Assert
        Assert.Equal(50, result.Count());
      }
    }

    [Fact]
    public void FindSql_Wrong_Parameter()
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
        var categoryRepository = (DapperRepository<Category>)CreateCategoryRepository(connection);
        categoryRepository.CreateMany(categories);

        // Act

        // Assert
        Assert.Throws(typeof(ArgumentException), () => 
          categoryRepository.Find("SELECT * FROM Category WHERE Id > @Id"));
        Assert.Throws(typeof(ArgumentException), () => 
          categoryRepository.Find("SELECT * FROM Category WHERE Id > @Id", 
            new Dictionary<string, object> { { "Wrong", 1 } }));
      }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SortBy(bool descendingOrder, bool useExpression)
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
        categoryRepository.CreateMany(categories);

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

        var result = categoryRepository.Find().ToList();

        // Assert
        var sortedCategories = descendingOrder
          ? result.OrderByDescending(p => p.Name)
          : result.OrderBy(p => p.Name);
        Assert.NotNull(result);
        Assert.Equal(result, sortedCategories);

        // Act
        result = categoryRepository
          .ClearSorting()
          .Find()
          .ToList();

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
    public void Page(int page, int pageSize, int totalRows, int expectedRows)
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
        categoryRepository.CreateMany(categories);

        // Act
        var pageItems = categoryRepository
          .Page(page, pageSize)
          .Find();

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(expectedRows, pageItems.Count());

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(expectedRows, pageItems.Count());

        // Act
        pageItems = categoryRepository
          .ClearPaging()
          .Find();

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
