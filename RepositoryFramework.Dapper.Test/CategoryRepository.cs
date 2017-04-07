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
  public class CategoryRepository : Repository<Category, CategoryFilter>
  {
    private Repository<Product, ProductFilter> productRepository = null;
    public CategoryRepository(IDbConnection connection,
      string lastRowIdCommand = "SELECT @@IDENTITY")
      : base(connection, lastRowIdCommand)
    {
      productRepository = new Repository<Product, ProductFilter>(connection, lastRowIdCommand);
    }

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

    public override Category GetById(string filter)
    {
      var category = base.GetById(filter);

      if (category == null)
      {
        return null;
      }

      category.Products = new List<Product>();
      var products = productRepository.Find(new ProductFilter { CategoryId = category.Id });

      if(products.TotalCount > 0)
      {
        foreach(var product in products.Items)
        {
          category.Products.Add(product);
        }
      }

      return category;
    }

    public override IQueryResult<Category> Find()
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var findQuery = $@"
SELECT * FROM Category c
OUTER LEFT JOIN Product p ON p.CategoryId = c.Id";

      var lookup = new Dictionary<int?, Category>();
      IEnumerable<Category> result = SqlMapper.Query<Category, Product, Category>(
        Connection, 
        findQuery, 
        (category, product) => Map(category, product, lookup));

      var categories = lookup.Values.AsEnumerable();
      return new QueryResult<Category>(categories, categories.Count());
    }

    public override IQueryResult<Category> Find(CategoryFilter filter)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var findQuery = $@"
SELECT * FROM Category
OUTER LEFT JOIN Product 
  ON Product.CategoryId = Category.Id
{CreateWhere(filter)}";

      var lookup = new Dictionary<int?, Category>();
      IEnumerable<Category> result = SqlMapper.Query<Category, Product, Category>(
        Connection,
        findQuery,
        (category, product) => Map(category, product, lookup),
        filter);

      var categories = lookup.Values.AsEnumerable();
      return new QueryResult<Category>(categories, categories.Count());
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
