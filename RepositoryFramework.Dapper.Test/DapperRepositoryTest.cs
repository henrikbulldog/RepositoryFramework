using System.Collections.Generic;
using System.Linq;
using Xunit;
using RepositoryFramework.Test.Models;
using System;
using RepositoryFramework.Test.Filters;
using Microsoft.Data.Sqlite;
using System.Data;
using RepositoryFramework.Interfaces;
using System.Threading.Tasks;

namespace RepositoryFramework.Dapper.Test
{
  public class DapperRepositoryTest
  {
    [Fact]
    public async Task GetById()
    {
      await GetByIdTest(
        async (pr, id) =>
        {
          return await Task.Run(() => pr.GetById(id));
        });
      await GetByIdTest(
        async (pr, id) =>
        {
          return await pr.GetByIdAsync(id);
        });
    }

    protected virtual async Task GetByIdTest(
      Func<IDapperRepository<Category>, object, Task<Category>> getById)
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
        await categoryRepository.CreateAsync(category);

        // Act
        var result = await getById(categoryRepository, category.Id.Value);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
        Assert.Equal(category.Description, result.Description);
      }
    }

    [Fact]
    public async Task CreateMany()
    {
      await CreateManyTest(
        async (r, e) =>
        {
          await Task.Run(() => r.CreateMany(e));
        });
      await CreateManyTest(
        async (r, e) =>
        {
          await r.CreateManyAsync(e);
        });
    }

    protected virtual async Task CreateManyTest(
      Func<IDapperRepository<Category>, IEnumerable<Category>, Task> createMany)
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
        await createMany(categoryRepository, createThese);

        // Assert
        var result = categoryRepository.Find();
        Assert.NotNull(result);
        Assert.Equal(totalCategories, result.Count());
      }
    }

    [Fact]
    public async Task Update()
    {
      await UpdateTest(
        async (r, e) =>
        {
          await Task.Run(() => r.Update(e));
        });
      await UpdateTest(
        async (r, e) =>
        {
          await r.UpdateAsync(e);
        });
    }

    protected virtual async Task UpdateTest(
      Func<IDapperRepository<Category>, Category, Task> update)
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
        await categoryRepository.CreateAsync(category);

        // Act
        category.Name = "New Name";
        category.Description = "New Description";
        await update(categoryRepository, category);

        // Assert
        var result = categoryRepository.GetById(category.Id.Value);
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
        Assert.Equal(category.Description, result.Description);
      }
    }

    [Fact]
    public async Task Delete()
    {
      await DeleteTest(
        async (r, e) =>
        {
          await Task.Run(() => r.Delete(e));
        });
      await DeleteTest(
        async (r, e) =>
        {
          await r.DeleteAsync(e);
        });
    }

    protected virtual async Task DeleteTest(
      Func<IDapperRepository<Category>, Category, Task> delete)
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
        categoryRepository.Create(category);
        var result = categoryRepository.GetById(category.Id.Value);

        // Act
        await delete(categoryRepository, category);

        // Assert
        result = categoryRepository.GetById(category.Id.Value);
        Assert.Null(result);
      }
    }

    [Fact]
    public async Task DeleteMany()
    {
      await DeleteManyTest(
        async (r, e) =>
        {
          await Task.Run(() => r.DeleteMany(e));
        });
      await DeleteManyTest(
        async (r, e) =>
        {
          await r.DeleteManyAsync(e);
        });
    }

    protected virtual async Task DeleteManyTest(
      Func<IDapperRepository<Category>, IEnumerable<Category>, Task> deleteMany)
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
          await categoryRepository.CreateAsync(category);
          if (i % 2 == 0)
          {
            deleteThese.Add(category);
          }
        }

        // Act
        await deleteMany(categoryRepository, deleteThese);

        // Assert
        var result = categoryRepository.Find();
        Assert.NotNull(result);
        Assert.Equal(totalCategories / 2, result.Count());
      }
    }

    [Fact]
    public async Task Find()
    {
      await FindTest(
        async (r) =>
        {
          return await Task.Run(() => r.Find());
        });
      await FindTest(
        async (r) =>
        {
          return await r.FindAsync();
        });
    }

    protected virtual async Task FindTest(
      Func<IDapperRepository<Category>, Task<IEnumerable<Category>>> find)
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
        var result = await find(categoryRepository);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rows, result.Count());
      }
    }

    [Fact]
    public async Task Combine_Page_Sort_Find()
    {
      await Combine_Page_Sort_Find_Test(
        async (r) =>
        {
          return await Task.Run(() => r.Find());
        });
      await Combine_Page_Sort_Find_Test(
        async (r) =>
        {
          return await r.FindAsync();
        });
    }

    protected virtual async Task Combine_Page_Sort_Find_Test(
      Func<IDapperRepository<Category>, Task<IEnumerable<Category>>> find)
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
        var result = await find(categoryRepository
          .Page(2, 40)
          .SortBy(c => c.Id));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(40, result.Count());
      }
    }


    [Fact]
    public async Task Combine_Page_Sort_FindSql()
    {
      await Combine_Page_Sort_FindSql_Test(
        async (r, sql, parameters) =>
        {
          return await Task.Run(() => r.Find(sql, parameters));
        });
      await Combine_Page_Sort_FindSql_Test(
        async (r, sql, parameters) =>
        {
          return await r.FindAsync(sql, parameters);
        });
    }

    protected virtual async Task Combine_Page_Sort_FindSql_Test(
      Func<IDapperRepository<Category>, string, Dictionary<string, object>, Task<IEnumerable<Category>>> find)
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
        var result = await find(categoryRepository
          .Page(2, 40)
          .SortBy(c => c.Id),
          "SELECT * FROM Category WHERE Id > @Id AND Description <> @Description",
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
    public async Task FindSql_No_Parameters()
    {
      await FindSql_No_Parameters_Test(
        async (r, sql) =>
        {
          return await Task.Run(() => r.Find(sql));
        });
      await FindSql_No_Parameters_Test(
        async (r, sql) =>
        {
          return await r.FindAsync(sql);
        });
    }

    protected virtual async Task FindSql_No_Parameters_Test(
      Func<IDapperRepository<Category>, string, Task<IEnumerable<Category>>> find)
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
        await categoryRepository.CreateManyAsync(categories);

        // Act
        var result = await find(categoryRepository, "SELECT * FROM Category");

        // Assert
        Assert.Equal(100, result.Count());
      }
    }

    [Fact]
    public async Task FindSql_With_Parameters()
    {
      await FindSql_With_Parameters_Test(
        async (r, sql, parameters) =>
        {
          return await Task.Run(() => r.Find(sql, parameters));
        });
      await FindSql_With_Parameters_Test(
        async (r, sql, parameters) =>
        {
          return await r.FindAsync(sql, parameters);
        });
    }

    protected virtual async Task FindSql_With_Parameters_Test(
      Func<IDapperRepository<Category>, string, Dictionary<string, object>, Task<IEnumerable<Category>>> find)
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
        await categoryRepository.CreateManyAsync(categories);

        // Act
        var result = await find(categoryRepository,
          "SELECT * FROM Category WHERE Id > @Id AND Description <> @Description",
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
    public async Task FindSql_With_Parameters_And_Pattern()
    {
      await FindSql_With_Parameters_And_Pattern_Test(
        async (r, sql, parameters, pattern) =>
        {
          return await Task.Run(() => r.Find(sql, parameters, pattern));
        });
      await FindSql_With_Parameters_And_Pattern_Test(
        async (r, sql, parameters, pattern) =>
        {
          return await r.FindAsync(sql, parameters, pattern);
        });
    }

    protected virtual async Task FindSql_With_Parameters_And_Pattern_Test(
      Func<IDapperRepository<Category>, string, Dictionary<string, object>, string, Task<IEnumerable<Category>>> find)
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
        await categoryRepository.CreateManyAsync(categories);

        // Act
        var result = await find(categoryRepository,
          "SELECT * FROM Category WHERE Id > :Id AND Description <> :Description",
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
    public async Task FindSql_Wrong_Parameter()
    {
      await FindSql_Wrong_Parameter_Test(
        async (r, sql, parameters) =>
        {
          return await Task.Run(() => r.Find(sql, parameters));
        });
      await FindSql_Wrong_Parameter_Test(
        async (r, sql, parameters) =>
        {
          return await r.FindAsync(sql, parameters);
        });
    }

    protected virtual async Task FindSql_Wrong_Parameter_Test(
      Func<IDapperRepository<Category>, string, Dictionary<string, object>, Task<IEnumerable<Category>>> find)
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
        await Assert.ThrowsAsync<ArgumentException>(async () =>
          await find(categoryRepository, "SELECT * FROM Category WHERE Id > @Id", null));
        await Assert.ThrowsAsync<ArgumentException>(async () =>
          await find(categoryRepository, "SELECT * FROM Category WHERE Id > @Id",
            new Dictionary<string, object> { { "Wrong", 1 } }));
      }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task SortBy(bool descendingOrder, bool useExpression)
    {
      await SortByTest(descendingOrder, useExpression,
        async (r) =>
        {
          return await Task.Run(() => r.Find());
        });
      await SortByTest(descendingOrder, useExpression,
        async (r) =>
        {
          return await r.FindAsync();
        });
    }

    protected virtual async Task SortByTest(bool descendingOrder, bool useExpression,
      Func<ISortableRepository<Category>, Task<IEnumerable<Category>>> find)
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

        var result = (await find(categoryRepository)).ToList();

        // Assert
        var sortedCategories = descendingOrder
          ? result.OrderByDescending(p => p.Name)
          : result.OrderBy(p => p.Name);
        Assert.NotNull(result);
        Assert.Equal(result, sortedCategories);
      }
    }

    [Theory]
    [InlineData(1, 40, 100, 40)]
    [InlineData(2, 40, 100, 40)]
    [InlineData(3, 40, 100, 20)]
    [InlineData(4, 40, 100, 0)]
    [InlineData(1, 0, 100, 100)]
    public async Task Page(int page, int pageSize, int totalRows, int expectedRows)
    {
      await PageTest(page, pageSize, totalRows, expectedRows,
        async (r) =>
        {
          return await Task.Run(() => r.Find());
        });
      await PageTest(page, pageSize, totalRows, expectedRows,
        async (r) =>
        {
          return await r.FindAsync();
        });
    }

    protected virtual async Task PageTest(int page, int pageSize, int totalRows, int expectedRows,
      Func<IPageableRepository<Category>, Task<IEnumerable<Category>>> find)
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
        var pageItems = await find(categoryRepository.Page(page, pageSize));

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(expectedRows, pageItems.Count());
        Assert.Equal(totalRows, categoryRepository.TotalItems);
        Assert.Equal(pageSize == 0 ? 1 : (totalRows / pageSize) + 1, categoryRepository.TotalPages);
        Assert.Equal(page, categoryRepository.PageNumber);
        Assert.Equal(page < 2 ? 1 : ((page - 1) * pageSize) + 1, categoryRepository.StartIndex);
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
