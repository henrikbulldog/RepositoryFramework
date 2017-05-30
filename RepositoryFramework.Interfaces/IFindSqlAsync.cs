using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Filters a sequence of entities using a predicate
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IFindSqlAsync<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Filters a collection of entities using a predicate
    /// </summary>
    /// <param name="sql">SQL containing named parameter placeholders. For example: SELECT * FROM Customer WHERE Id = @Id</param>
    /// <param name="parameters">Named parameters</param>
    /// <param name="parameterPattern">Parameter Regex pattern, Defualts to @(\w+)</param>
    /// <returns>Filtered collection of entities</returns>
    Task<IEnumerable<TEntity>>  FindAsync(
      string sql,
      IDictionary<string, object> parameters = null,
      string parameterPattern = @"@(\w+)");
  }
}
