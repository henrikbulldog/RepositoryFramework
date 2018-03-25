using System.Collections.Generic;
using System.Linq;
using Xunit;
using RepositoryFramework.Test.Models;
using System;
using RepositoryFramework.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using RepositoryFramework.Interfaces;
using Microsoft.Data.Sqlite;
using Xunit.Abstractions;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace RepositoryFramework.Test
{
  public class EntityFrameworkRepositoryTest : TestLogger
  {
    public EntityFrameworkRepositoryTest(ITestOutputHelper output)
      : base(output)
    {
    }

    [Theory]
    [InlineData("", 100, 0, 0)]
    [InlineData("Products", 100, 100, 0)]
    [InlineData("pRoducts", 100, 100, 0)]
    [InlineData("Products.Parts", 100, 100, 100)]
    [InlineData("Products.Parts,Products.Parts.Product", 100, 100, 100)]
    public async Task Include(string includes, int productRows, int expectedProductRows, int expectedPartRows)
    {
      await IncludeTest(includes, productRows, expectedProductRows, expectedPartRows,
        async (categoryRepository, id) =>
        {
          return await Task.Run(() => categoryRepository.GetById(id));
        });
      await IncludeTest(includes, productRows, expectedProductRows, expectedPartRows,
        async (categoryRepository, id) =>
        {
          return await categoryRepository.GetByIdAsync(id);
        });
    }

    protected async Task IncludeTest(
      string includes,
      int productRows,
      int expectedProductRows,
      int expectedPartRows,
      Func<IEntityFrameworkRepository<Category>, int?, Task<Category>> getById)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Category> categoryRepository = new EntityFrameworkRepository<Category>(db)
          .Include(includes);
        var category = CreateCategory(productRows);
        categoryRepository.Create(category);
        // Detach all to avoid expansions already cached in the context
        categoryRepository.DetachAll();

        // Act 
        category = await getById(categoryRepository, category.Id);

        // Assert
        Assert.NotNull(category);
        if (category.Products == null)
        {
          Assert.Equal(0, expectedProductRows);
        }
        else
        {
          Assert.Equal(expectedProductRows, category.Products.Count);
          var product = category.Products.First();
          if (product.Parts == null)
          {
            Assert.Equal(0, expectedPartRows);
          }
          else
          {
            Assert.Equal(expectedPartRows, product.Parts.Count);
          }
        }
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
        async (products) =>
        {
          return await Task.Run(() => products.Find());
        });
      await SortByTest(descendingOrder, useExpression,
        async (products) =>
        {
          return await products.FindAsync();
        });
    }

    protected virtual async Task SortByTest(
      bool descendingOrder,
      bool useExpression,
      Func<ISortableRepository<Product>, Task<IEnumerable<Product>>> find)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        ISortableRepository<Category> cr = new EntityFrameworkRepository<Category>(db);
        var c = CreateCategory(100);
        cr.Create(c);
        ISortableRepository<Product> unsortedProductRepository = new EntityFrameworkRepository<Product>(db);
        ISortableRepository<Product> sortedProductRepository = new EntityFrameworkRepository<Product>(db);
        if (descendingOrder)
        {
          if (useExpression)
          {
            sortedProductRepository.SortByDescending("Name");
          }
          else
          {
            sortedProductRepository.SortByDescending(p => p.Name);
          }
        }
        else
        if (useExpression)
        {
          sortedProductRepository.SortBy("Name");
        }
        else
        {
          sortedProductRepository.SortBy(p => p.Name);
        }

        // Act
        var sortedProducts = await find(sortedProductRepository);
        var unsortedProducts = await find(unsortedProductRepository);

        // Assert
        var sortedList = descendingOrder
          ? sortedProducts.OrderByDescending(p => p.Name)
          : sortedProducts.OrderBy(p => p.Name);
        Assert.NotNull(sortedProducts);
        Assert.Equal(sortedProducts, sortedList);
        Assert.NotNull(unsortedProducts);
        Assert.NotEqual(unsortedProducts, sortedList);
      }
    }

    [Theory]
    [InlineData(1, 40, 100, 40)]
    [InlineData(2, 40, 100, 40)]
    [InlineData(3, 40, 100, 20)]
    [InlineData(4, 40, 100, 0)]
    [InlineData(1, 0, 100, 100)]
    [InlineData(1, 40, 1000, 40)]
    public async Task Page(int page, int pageSize, int totalRows, int expectedRows)
    {
      await PageTest(page, pageSize, totalRows, expectedRows,
        async (products) =>
        {
          return await Task.Run(() => products.Find());
        });
      await PageTest(page, pageSize, totalRows, expectedRows,
        async (products) =>
        {
          return await products.FindAsync();
        });
    }

    protected virtual async Task PageTest(
      int page,
      int pageSize,
      int totalRows,
      int expectedRows,
      Func<IPageableRepository<Product>, Task<IEnumerable<Product>>> find)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IPageableRepository<Category> cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(totalRows);
        cr.Create(category);
        IPageableRepository<Product> pr = new EntityFrameworkRepository<Product>(db)
          .Page(page, pageSize);

        // Act
        var pageItems = await find(pr);

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(expectedRows, pageItems.Count());
        Assert.Equal(totalRows, pr.TotalItems);
        Assert.Equal(pageSize == 0 ? 1 : (totalRows / pageSize) + 1, pr.TotalPages);
        Assert.Equal((page * pageSize) + 1, pr.StartIndex);
      }
    }

    [Fact]
    public async Task Combine_Page_Sort_Include_Find()
    {
      await Combine_Page_Sort_Include_Find_Test(
        async (products) =>
        {
          return await Task.Run(() => products.Find());
        });
      await Combine_Page_Sort_Include_Find_Test(
        async (products) =>
        {
          return await products.FindAsync();
        });
    }

    protected virtual async Task Combine_Page_Sort_Include_Find_Test(
      Func<IPageableRepository<Product>, Task<IEnumerable<Product>>> find)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Category> cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(100);
        cr.Create(category);
        IEntityFrameworkRepository<Product> pr = new EntityFrameworkRepository<Product>(db)
          .Page(2, 40)
          .Include("Parts")
          .SortBy("Name");

        // Act
        var pageItems = await find(pr);

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(40, pageItems.Count());
        Assert.Equal(100, pr.TotalItems);
      }
    }

    [Fact]
    public async Task Combine_Page_Sort_Include_FindSql()
    {
      await Combine_Page_Sort_Include_FindSql_Test(
        async (products, sql, parameters) =>
        {
          return await Task.Run(() => products.Find(sql, parameters));
        });
      await Combine_Page_Sort_Include_FindSql_Test(
        async (products, sql, parameters) =>
        {
          return await products.FindAsync(sql, parameters);
        });
    }

    protected virtual async Task Combine_Page_Sort_Include_FindSql_Test(
      Func<IEntityFrameworkRepository<Product>, string, Dictionary<string, object>, Task<IEnumerable<Product>>> find)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Category> cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(100);
        cr.Create(category);
        IEntityFrameworkRepository<Product> pr = new EntityFrameworkRepository<Product>(db)
          .Page(2, 40)
          .Include("Parts")
          .SortBy("Name");

        // Act
        var pageItems = await find(pr, "SELECT * FROM Product WHERE Id > @Id AND Description <> @Description",
          new Dictionary<string, object>
          {
            { "Description", "XXX" },
            { "Id", 50 }
          });

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(50, pr.TotalItems);
        Assert.Equal(10, pageItems.Count());
      }
    }

    [Fact]
    public async Task FindWhere()
    {
      await FindWhereTest(
        async (products, where) =>
        {
          return await Task.Run(() => products.Find(where));
        });
      await FindWhereTest(
        async (products, where) =>
        {
          return await products.FindAsync(where);
        });
    }

    protected virtual async Task FindWhereTest(
      Func<IEntityFrameworkRepository<Product>, Expression<Func<Product, bool>>, Task<IEnumerable<Product>>> find)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Category> cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(100);
        cr.Create(category);
        IEntityFrameworkRepository<Product> pr = new EntityFrameworkRepository<Product>(db)
          .Include("Parts");

        // Act
        var pageItems = await find(pr, p => p.Id > 50);

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(50, pageItems.Count());
      }
    }

    [Fact]
    public async Task AsQueryable()
    {
      await AsQueryableTest(
        async (q) =>
        {
          return await Task.Run(() => q.ToList());
        });
      await AsQueryableTest(
        async (q) =>
        {
          return await q.ToListAsync();
        });
    }

    protected virtual async Task AsQueryableTest(
      Func<IIncludableQueryable<Product, ICollection<Part>>, Task<List<Product>>> toList)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Category> cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(100);
        cr.Create(category);
        IEntityFrameworkRepository<Product> pr = new EntityFrameworkRepository<Product>(db);
        var q = pr
          .AsQueryable()
          .Where(p => p.Id > 1)
          .Skip(2)
          .Take(40)
          .OrderBy(p => p.Name)
          .Include(p => p.Parts);

        // Act
        var pageItems = await toList(q);

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(40, pageItems.Count());
      }
    }

    [Fact]
    public async Task FindSql_No_Parameters()
    {
      await FindSql_No_Parameters_Test(
        async (products, sql) =>
        {
          return await Task.Run(() => products.Find(sql));
        });
      await FindSql_No_Parameters_Test(
        async (products, sql) =>
        {
          return await products.FindAsync(sql);
        });
    }

    protected virtual async Task FindSql_No_Parameters_Test(
      Func<IEntityFrameworkRepository<Product>, string, Task<IEnumerable<Product>>> find)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        var cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(100);
        cr.Create(category);

        // Act
        var pr = new EntityFrameworkRepository<Product>(db);
        var result = await find(pr, "SELECT * FROM Product");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.Count());
      }
    }

    [Fact]
    public async Task FindSql_With_Parameters()
    {
      await FindSql_With_Parameters_Test(
        async (products, sql, parameters) =>
        {
          return await Task.Run(() => products.Find(sql, parameters));
        });
      await FindSql_With_Parameters_Test(
        async (products, sql, parameters) =>
        {
          return await products.FindAsync(sql, parameters);
        });
    }

    protected virtual async Task FindSql_With_Parameters_Test(
      Func<IEntityFrameworkRepository<Product>, string, Dictionary<string, object>, Task<IEnumerable<Product>>> find)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        var cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(100);
        cr.Create(category);

        // Act
        var pr = new EntityFrameworkRepository<Product>(db);
        var result = await find(pr, "SELECT * FROM Product WHERE Id > @Id AND Description <> @Description",
          new Dictionary<string, object>
          {
            { "Description", "XXX" },
            { "Id", 50 }
          });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50, result.Count());
      }
    }

    [Fact]
    public async Task FindSql_With_Parameters_And_Pattern()
    {
      await FindSql_With_Parameters_And_Pattern_Test(
        async (products, sql, parameters, pattern) =>
        {
          return await Task.Run(() => products.Find(sql, parameters, pattern));
        });
      await FindSql_With_Parameters_And_Pattern_Test(
        async (products, sql, parameters, pattern) =>
        {
          return await products.FindAsync(sql, parameters, pattern);
        });
    }

    protected virtual async Task FindSql_With_Parameters_And_Pattern_Test(
      Func<IEntityFrameworkRepository<Product>, string, Dictionary<string, object>, string, Task<IEnumerable<Product>>> find)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        var cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(100);
        cr.Create(category);

        // Act
        var pr = new EntityFrameworkRepository<Product>(db);
        var result = await find(pr, "SELECT * FROM Product WHERE Id > :Id AND Description <> :Description",
          new Dictionary<string, object>
          {
            { "Description", "XXX" },
            { "Id", 50 }
          },
          @":(\w+)");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50, result.Count());
      }
    }

    [Fact]
    public async Task FindSql_Wrong_Parameter()
    {
      await FindSql_Wrong_Parameter_Test(
        async (products, sql, parameters) =>
        {
          return await Task.Run(() => products.Find(sql, parameters));
        });
      await FindSql_Wrong_Parameter_Test(
        async (products, sql, parameters) =>
        {
          return await products.FindAsync(sql, parameters);
        });
    }

    protected virtual async Task FindSql_Wrong_Parameter_Test(
      Func<IEntityFrameworkRepository<Product>, string, Dictionary<string, object>, Task<IEnumerable<Product>>> find)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        var cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(100);
        cr.Create(category);
        var pr = new EntityFrameworkRepository<Product>(db);
        var sql = "SELECT * FROM Product WHERE Id > @Id";
        var parameters = new Dictionary<string, object> { { "Wrong", 1 } };

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await find(pr, sql, null));
        await Assert.ThrowsAsync<ArgumentException>(async () => await find(pr, sql, parameters));
      }
    }

    [Fact]
    public async Task Combine_Page_Sort_Include_AsQueryable()
    {
      await Combine_Page_Sort_Include_AsQueryable_Test(
        async (q) =>
        {
          return await Task.Run(() => q.ToList());
        });
      await Combine_Page_Sort_Include_AsQueryable_Test(
        async (q) =>
        {
          return await q.ToListAsync();
        });
    }

    protected virtual async Task Combine_Page_Sort_Include_AsQueryable_Test(
      Func<IQueryable<Product>, Task<List<Product>>> toList)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Category> cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(100);
        cr.Create(category);
        IEntityFrameworkRepository<Product> pr = new EntityFrameworkRepository<Product>(db);
        var q = pr
          .Page(2, 40)
          .Include("Parts")
          .SortBy("Name")
          .AsQueryable();

        // Act
        var pageItems = await toList(q);

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(40, pageItems.Count());
      }
    }

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
      Func<IEntityFrameworkRepository<Product>, object, Task<Product>> getById)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Category> cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(100);
        cr.Create(category);
        IEntityFrameworkRepository<Product> pr = new EntityFrameworkRepository<Product>(db)
          .Include(p => p.Category)
          .Include(p => p.Parts);

        // Act
        var product = await getById(pr, 1);

        // Assert
        Assert.NotNull(product);
        Assert.Equal(1, product.Id);
        Assert.NotNull(product.Category);
        Assert.NotNull(product.Parts);
      }
    }

    [Fact]
    public async Task GetById_Alternative_Key()
    {
      await GetById_Alternative_Key_Test(
        async (pr, id) =>
        {
          return await Task.Run(() => pr.GetById(id));
        });
      await GetById_Alternative_Key_Test(
        async (pr, id) =>
        {
          return await pr.GetByIdAsync(id);
        });
    }

    protected virtual async Task GetById_Alternative_Key_Test(
      Func<IEntityFrameworkRepository<Order>, object, Task<Order>> getById)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Order> r =
          new EntityFrameworkRepository<Order>(db, o => o.OrderKey);
        var order = new Order { OrderDate = DateTime.Now };
        r.Create(order);

        // Act
        var result = await getById(r, order.OrderKey);

        // Assert
        Assert.True(order.OrderKey > 0);
        Assert.NotNull(result);
      }
    }

    private Product CreateProduct(Category c, int partRows = 100)
    {
      var p = new Product
      {
        Name = Guid.NewGuid().ToString().ToLower().Replace("aa", "ab"),
        Category = c,
        Description = Guid.NewGuid().ToString(),
        Price = 1.23M
      };
      var l = new List<Part>();
      for (int i = 0; i < partRows; i++)
      {
        l.Add(CreatePart(p));
      };
      p.Parts = l;
      return p;
    }

    private Part CreatePart(Product p)
    {
      return new Part
      {
        Name = Guid.NewGuid().ToString().ToLower().Replace("aa", "ab"),
        Product = p,
        Description = Guid.NewGuid().ToString(),
        Price = 1.23M
      };
    }

    private Category CreateCategory(int productRows = 100)
    {
      var c = new Category
      {
        Name = Guid.NewGuid().ToString(),
        Description = Guid.NewGuid().ToString()
      };
      var products = new List<Product>();
      for (int i = 0; i < productRows; i++)
      {
        products.Add(CreateProduct(c));
      };
      c.Products = products;
      return c;
    }
  }
}
