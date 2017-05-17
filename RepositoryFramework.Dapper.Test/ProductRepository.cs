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
  public class ProductRepository : DapperRepository<Product>
  {
    public ProductRepository(IDbConnection connection,
      string lastRowIdCommand = "SELECT @@IDENTITY")
      : base(connection, lastRowIdCommand)
    {
    }

    public IEnumerable<Product> FindByCategoryId(object categoryId)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var findQuery = $@"
SELECT * FROM Product
WHERE CategoryId = @CategoryId";

      return Connection.Query<Product>(
        findQuery,
        new { CategoryId = categoryId });
    }
  }
}
