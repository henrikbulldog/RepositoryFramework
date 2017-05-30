using Microsoft.EntityFrameworkCore;
using RepositoryFramework.EntityFramework;
using RepositoryFramework.Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryFramework.Test
{
  public class EFCoreTest
  {
    [Fact]
    public void EFCore_Should_Not_Support_Lazy_Load()
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        var cr = new EntityFrameworkRepository<Category>(db, null, false);
        var c = CreateCategory(100);
        cr.Create(c);
        cr.SaveChanges();

        // Detach all to avoid expansions already cached in the context
        cr.DetachAll();

        // Act
        var cdb = cr.Find().First();
        Assert.NotNull(cdb);

        // Assert
        // Looks like lazy loading is not supported in EF Core
        Assert.Null(cdb.Products);
      }
    }

    [Fact]
    public void EFCore_Should_Support_Circular_References()
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        var cr = new EntityFrameworkRepository<Category>(db, null, false);
        var c = CreateCategory(100);
        cr.Create(c);
        cr.SaveChanges();

        // Detach all to avoid expansions already cached in the context
        cr.DetachAll();

        // Act
        var cdb = cr
          .Include("Products.Parts.Product")
          .Find().First();

        Assert.NotNull(cdb);
        var products = cdb.Products;

        // Assert
        Assert.NotNull(products);
        Assert.Equal(100, products.Count);
        var product = products.First();
        for (int i = 0; i < 10; i++)
        {
          Assert.NotNull(product);
          Assert.NotNull(product.Parts);
          var part = product.Parts.First();
          product = part.Product;
        }
      }
    }

    [Fact]
    public void Join_Should_Work_Without_Expansion()
    {
      // Create new empty database
      using (var db = new SQLiteContext())
      {
        // Arrange
        var cr = new EntityFrameworkRepository<Category>(db);
        var c = CreateCategory(100);
        cr.Create(c);

        // Act
        var pr = new EntityFrameworkRepository<Part>(db);
        var pdb = pr
          .Find(p => p.Product.Id == 1).First();

        Assert.NotNull(pdb);

        // Assert
        Assert.NotNull(pdb);
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
  }
}
