using System.Collections.Generic;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Query result set.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public class QueryResult<TEntity> : IQueryResult<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryResult{TEntity}"/> class.
    /// </summary>
    /// <param name="items">Items</param>
    /// <param name="totalCount">Number of items</param>
    public QueryResult(IEnumerable<TEntity> items, int totalCount)
    {
      Items = items;
      TotalCount = totalCount;
    }

    /// <summary>
    /// Gets items
    /// </summary>
    public IEnumerable<TEntity> Items { get; }

    /// <summary>
    /// Gets the number of items
    /// </summary>
    public int TotalCount { get; }
  }
}
