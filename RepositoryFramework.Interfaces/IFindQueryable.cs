using System;
using System.Linq;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Finds a list of entites using a lambda expression for filtering.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IFindQueryable<TEntity> : IFindConstraints<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Finds a list of entites using a filter expression.
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <returns>Current instance</returns>
    IQueryResult<TEntity> Find(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter);

    /// <summary>
    /// Finds a list of entites using a filter expression and query constraints for expansion, paging and sorting.
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <param name="constraints">Query constraints for expansion, paging and sorting</param>
    /// <returns>Current instance</returns>
    IQueryResult<TEntity> Find(
      Func<IQueryable<TEntity>,
      IQueryable<TEntity>> filter, IQueryConstraints<TEntity> constraints);
  }
}