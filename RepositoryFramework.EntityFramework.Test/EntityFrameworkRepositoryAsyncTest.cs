using System.Collections.Generic;
using System.Linq;
using Xunit;
using RepositoryFramework.Test.Models;
using System;
using RepositoryFramework.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace RepositoryFramework.Test
{
  public class EntityFrameworkRepositoryAsyncTest
  {
    [Theory]
    [InlineData("", 100, 0, 0)]
    [InlineData("Products", 100, 100, 0)]
    [InlineData("pRoducts", 100, 100, 0)]
    [InlineData("Products.Parts", 100, 100, 100)]
    [InlineData("Products.Parts,Products.Parts.Product", 100, 100, 100)]
    public async Task IncludeAsync(string includes, int productRows, int expectedProductRows, int expectedPartRows)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Category> categoryRepository = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(productRows);
        await categoryRepository.CreateAsync(category);

        // Detach all to avoid expansions already cached in the context
        await categoryRepository.DetachAllAsync();

        // Act
        category = await categoryRepository
          .Include(includes)
          .GetByIdAsync(category.Id);

        // Assert
        Assert.NotNull(category);
        Include_AssertProduct(category.Products, expectedProductRows, expectedPartRows);

        // Act
        await categoryRepository.DetachAllAsync();
        category = categoryRepository
          .Include(includes)
          .GetById(1);

        // Assert
        Assert.NotNull(category);
        Include_AssertProduct(category.Products, expectedProductRows, expectedPartRows);

        // Act
        await categoryRepository.DetachAllAsync();
        category = categoryRepository
          .ClearIncludes()
          .GetById(1);

        // Assert
        Assert.NotNull(category);
        Include_AssertProduct(category.Products, 0, expectedPartRows);
      }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task SortByAsync(bool descendingOrder, bool useExpression)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Category> cr = new EntityFrameworkRepository<Category>(db);
        var c = CreateCategory(100);
        cr.Create(c);

        // Act
        IEntityFrameworkRepository<Product> pr = new EntityFrameworkRepository<Product>(db);
        if (descendingOrder)
        {
          if (useExpression)
          {
            pr.SortByDescending("Name");
          }
          else
          {
            pr.SortByDescending(p => p.Name);
          }
        }
        else
        if (useExpression)
        {
          pr.SortBy("Name");
        }
        else
        {
          pr.SortBy(p => p.Name);
        }

        var products = pr.Find().ToList();

        var sortedproducts = descendingOrder
          ? products.OrderByDescending(p => p.Name)
          : products.OrderBy(p => p.Name);

        // Assert
        Assert.NotNull(products);
        Assert.Equal(products, sortedproducts);

        // Act
        products = (await pr.ClearSorting().FindAsync()).ToList();

        // Assert
        Assert.NotNull(products);
        Assert.NotEqual(products, sortedproducts);
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
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Category> cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(totalRows);
        cr.Create(category);

        // Act
        IEntityFrameworkRepository<Product> pr = new EntityFrameworkRepository<Product>(db);
        var pageItems = await pr
          .Page(page, pageSize)
          .FindAsync();

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(expectedRows, pageItems.Count());

        // Act
        pageItems = await pr
          .ClearPaging()
          .FindAsync();

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(totalRows, pageItems.Count());
      }
    }

    [Fact]
    public async Task Combine_Page_Sort_IncludeAsync()
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Category> cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(100);
        cr.Create(category);

        // Act
        IEntityFrameworkRepository<Product> pr = new EntityFrameworkRepository<Product>(db);
        var pageItems = await pr
          .Page(2, 40)
          .SortBy("Name")
          .Include("Parts")
          .FindAsync();

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(40, pageItems.Count());
      }
    }

    [Fact]
    public async Task AsQueryableAsync()
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Category> cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(100);
        cr.Create(category);

        // Act
        IEntityFrameworkRepository<Product> pr = new EntityFrameworkRepository<Product>(db);
        var pageItems = await pr
          .AsQueryable()
          .Where(p => p.Id > 1)
          .Skip(2)
          .Take(40)
          .OrderBy(p => p.Name)
          .Include(p => p.Parts)
          .ToListAsync();

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(40, pageItems.Count());
      }
    }

    [Fact]
    public async Task Combine_Page_Sort_Include_AsQueryableAsync()
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Category> cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(100);
        cr.Create(category);

        // Act
        IEntityFrameworkRepository<Product> pr = new EntityFrameworkRepository<Product>(db);
        var pageItems = await pr
          .Page(2, 40)
          .SortBy("Name")
          .Include("Parts")
          .AsQueryable()
          .ToListAsync();

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(40, pageItems.Count());
      }
    }

    [Fact]
    public async Task FindWhereAsync()
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Category> cr = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(100);
        cr.Create(category);

        // Act
        IEntityFrameworkRepository<Product> pr = new EntityFrameworkRepository<Product>(db);
        var pageItems = await pr
          .Include("Parts")
          .FindAsync(p => p.Id > 50);

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(50, pageItems.Count());
      }
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

    private Product CreateProduct(Category c, int partRows=100)
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

    private void Include_AssertProduct(ICollection<Product> products, int expectedProductRows, int expectedPartRows)
    {
      if (products == null)
      {
        Assert.Equal(0, expectedProductRows);
      }
      else
      {
        Assert.Equal(expectedProductRows, products.Count);
        var product = products.First();
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
}
