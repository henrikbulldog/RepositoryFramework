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
  public class EntityFrameworkRepositoryTest
  {
    [Theory]
    [InlineData("", 100, 0, 0)]
    [InlineData("Products", 100, 100, 0)]
    [InlineData("pRoducts", 100, 100, 0)]
    [InlineData("Products.Parts", 100, 100, 100)]
    [InlineData("Products.Parts,Products.Parts.Product", 100, 100, 100)]
    public void Include(string includes, int productRows, int expectedProductRows, int expectedPartRows)
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        IEntityFrameworkRepository<Category> categoryRepository = new EntityFrameworkRepository<Category>(db);
        var category = CreateCategory(productRows);
        categoryRepository.Create(category);

        // Detach all to avoid expansions already cached in the context
        categoryRepository.DetachAll();

        // Act
        category = categoryRepository
          .Include(includes)
          .Find()
          .First();

        // Assert
        Assert.NotNull(category);
        Include_AssertProduct(category.Products, expectedProductRows, expectedPartRows);

        // Act
        categoryRepository.DetachAll();
        category = categoryRepository
          .Include(includes)
          .GetById(1);

        // Assert
        Assert.NotNull(category);
        Include_AssertProduct(category.Products, expectedProductRows, expectedPartRows);

        // Act
        categoryRepository.DetachAll();
        category = categoryRepository
          .ClearIncludes()
          .GetById(1);

        // Assert
        Assert.NotNull(category);
        Include_AssertProduct(category.Products, 0, expectedPartRows);
      }
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

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SortBy(bool descendingOrder, bool useExpression)
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
        products = pr
          .ClearSorting()
          .Find()
          .ToList();

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
    public void Page(int page, int pageSize, int totalRows, int expectedRows)
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
        var pageItems = pr
          .Page(page, pageSize)
          .Find();

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(expectedRows, pageItems.Count());

        // Act
        pageItems = pr
          .ClearPaging()
          .Find();

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(totalRows, pageItems.Count());
      }
    }

    [Fact]
    public void Combine_Page_Sort_Include()
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
        var pageItems = pr
          .Page(2, 40)
          .SortBy("Name")
          .Include("Parts")
          .Find();

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(40, pageItems.Count());
      }
    }

    [Fact]
    public void FindWhere()
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
        var pageItems = pr
          .Include("Parts")
          .Find(p => p.Id > 50);

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(50, pageItems.Count());
      }
    }

    [Fact]
    public void AsQueryable()
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
        var pageItems = pr
          .AsQueryable()
          .Where(p => p.Id > 1)
          .Skip(2)
          .Take(40)
          .OrderBy(p => p.Name)
          .Include(p => p.Parts);

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(40, pageItems.Count());
      }
    }

    [Fact]
    public void Combine_Page_Sort_Include_AsQueryable()
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
        var pageItems = pr
          .Page(2, 40)
          .SortBy("Name")
          .Include("Parts")
          .AsQueryable();

        // Assert
        Assert.NotNull(pageItems);
        Assert.Equal(40, pageItems.Count());
      }
    }

    [Fact]
    public void GetById()
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

        var products = pr.Find(p => p.Id == 1);
        var product = pr.GetById(1);

        // Assert
        Assert.NotNull(product);
      }
    }

    [Fact]
    public void GetById_Alternative_Key()
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
        var result = r.GetById(order.OrderKey);

        // Assert
        Assert.True(order.OrderKey > 0);
        Assert.NotNull(result);
      }
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
