using System;
using System.Collections.Generic;
using System.Linq;
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
  }
}
