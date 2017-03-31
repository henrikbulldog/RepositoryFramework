using System.Collections.Generic;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Query result set.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IQueryResult<out TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Gets all matching items
    /// </summary>
    IEnumerable<TEntity> Items { get; }

    /// <summary>
    /// Gets total number of items (useful when paging is used)
    /// </summary>
    int TotalCount { get; }
  }
}
