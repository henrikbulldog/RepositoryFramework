using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Repository that supports IQueryable
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IQueryableRepository<TEntity> :
    IRepository<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Gets a queryable collection of entities
    /// </summary>
    /// <returns>Queryable collection of entities</returns>
    IQueryable<TEntity> AsQueryable();

    /// <summary>
    /// Filters a collection of entities using a predicate using deferred execution
    /// </summary>
    /// <param name="where">Where predicate</param>
    /// <returns>Filtered collection of entities for deferred execution</returns>
    IQueryableRepository<TEntity> Where(Expression<Func<TEntity, bool>> where);

    /// <summary>
    /// Clear filters
    /// </summary>
    /// <returns>Current instance</returns>
    IQueryableRepository<TEntity> ClearWhere();
  }
}
