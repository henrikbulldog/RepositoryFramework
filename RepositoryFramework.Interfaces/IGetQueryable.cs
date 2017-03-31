using System;
using System.Linq;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Gets an entity by a lambda expression.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IGetQueryable<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Gets an entity by a filter expression.
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <returns>Entity</returns>
    TEntity GetById(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter);

    /// <summary>
    /// Gets an entity by a filter expression and query constraints for exapanding (eager loading) navigatinal properties and collections.
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <param name="includes">Query constraints for exapanding (eager loading) navigational properties and collections</param>
    /// <returns>Entity</returns>
    TEntity GetById(
      Func<IQueryable<TEntity>,
      IQueryable<TEntity>> filter,
      IExpandable<TEntity> includes);
  }
}
