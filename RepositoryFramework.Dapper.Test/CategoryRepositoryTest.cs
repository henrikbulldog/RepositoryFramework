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
  public class CategoryRepositoryTest
  {
    private void InitializeDatabase(SqliteConnection connection)
    {
      if (connection.State != ConnectionState.Open)
      {
        connection.Open();
      }

      var c = new SqliteCommand(@"
CREATE TABLE Category 
(
  Id INTEGER PRIMARY KEY,
  Name NVARCHAR(100),
  NullField NVARCHAR(100) DEFAULT NULL,
  DateTimeField DATETIME,
  Description NVARCHAR(100)
);

CREATE TABLE Product 
(
  Id INTEGER PRIMARY KEY,
  Name NVARCHAR(100),
  Description NVARCHAR(100),
  Price FLOAT,
  CategoryId INTEGER
)", connection);

      c.ExecuteNonQuery();
    }

    [Fact]
    public void Create_And_GetById()
    {
      using (var connection = new SqliteConnection("Data Source=:memory:"))
      {
        InitializeDatabase(connection);

        // Arrange
        IDapperRepository<Category> categoryRepository = 
          new CategoryRepository(connection, "SELECT last_insert_rowid()");
        var category = new Category
        {
          Name = "Category 1",
          Products = new List<Product>
          {
            new Product { Name = "Product 1" },
            new Product { Name = "Product 2" }
          }
        };

        // Act
        categoryRepository.Create(category);

        // Assert
        var result = categoryRepository.GetById(category.Id.Value);
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
        Assert.NotNull(result.Products);
        Assert.Equal(2, result.Products.Count);
        Assert.False(result.Products.Any(p => p.CategoryId != category.Id));
      }
    }

    [Fact]
    public void Update()
    {
      using (var connection = new SqliteConnection("Data Source=:memory:"))
      {
        InitializeDatabase(connection);

        // Arrange
        IDapperRepository<Category> categoryRepository = 
          new CategoryRepository(connection, "SELECT last_insert_rowid()");
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
      using (var connection = new SqliteConnection("Data Source=:memory:"))
      {
        InitializeDatabase(connection);

        // Arrange
        IDapperRepository<Category> categoryRepository = 
          new CategoryRepository(connection, "SELECT last_insert_rowid()");
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
    [InlineData(100, 2)]
    [InlineData(100, 0)]
    public void Find(int categories, int products)
    {
      using (var connection = new SqliteConnection("Data Source=:memory:"))
      {
        InitializeDatabase(connection);

        // Arrange
        IDapperRepository<Category> categoryRepository = 
          new CategoryRepository(connection, "SELECT last_insert_rowid()");
        for (int i = 0; i < categories; i++)
        {
          var category = new Category
          {
            Name = i.ToString(),
            Description = i.ToString(),
            Products = CreateProducts(products)
          };
          categoryRepository.Create(category);
        }

        // Act
        var result = categoryRepository.Find();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categories, result.Count());
        foreach (var category in result)
        {
          Assert.Equal(products, category.Products.Count);
        }
      }
    }

    private ICollection<Product> CreateProducts(int products)
    {
      var productList = new List<Product>();
      for (var n = 0; n < products; n++)
      {
        productList.Add(new Product { Name = $"Product {n}" });
      }
      return productList;
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
      return c;
    }
  }
}
