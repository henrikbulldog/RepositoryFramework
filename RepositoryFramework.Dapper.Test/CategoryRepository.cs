using RepositoryFramework.Test.Filters;
using RepositoryFramework.Test.Models;
using System.Collections.Generic;
using System.Data;
using RepositoryFramework.Interfaces;
using Dapper;
using System.Linq;
using System;

namespace RepositoryFramework.Dapper.Test
{
  public class CategoryRepository : DapperRepository<Category>
  {
    public CategoryRepository(IDbConnection connection,
      string lastRowIdCommand = "SELECT @@IDENTITY")
      : base(connection, lastRowIdCommand)
    {
      productRepository = new ProductRepository(connection, lastRowIdCommand);
    }

    private ProductRepository productRepository = null;

    public override void Create(Category entity)
    {
      base.Create(entity);
      if(entity.Products == null)
      {
        return;
      }
      foreach (var product in entity.Products)
      {
        product.CategoryId = entity.Id;
        productRepository.Create(product);
      }
    }

    public override Category GetById(object id)
    {
      var category = base.GetById(id);

      if (category == null)
      {
        return null;
      }

      category.Products = new List<Product>();
      var products = productRepository
        .FindByCategoryId(id);

      if(products.Count() > 0)
      {
        foreach(var product in products)
        {
          category.Products.Add(product);
        }
      }

      return category;
    }

    public override IEnumerable<Category> Find()
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var findQuery = $@"
SELECT * FROM Category
OUTER LEFT JOIN Product 
  ON Product.CategoryId = Category.Id";

      var lookup = new Dictionary<int?, Category>();
      IEnumerable<Category> result = SqlMapper.Query<Category, Product, Category>(
        Connection,
        findQuery,
        (category, product) => Map(category, product, lookup));

      return lookup.Values.AsEnumerable();
    }

    private Category Map(Category category, Product product, Dictionary<int?, Category> lookup)
    {
      Category currentCategory;

      if (!lookup.TryGetValue(category.Id, out currentCategory))
      {
        currentCategory = category;
        lookup.Add(category.Id, currentCategory);
      }

      if (currentCategory.Products == null)
      {
        currentCategory.Products = new List<Product>();
      }

      if (product != null)
      {
        currentCategory.Products.Add(product);
      }
      return currentCategory;
    }
  }
}
