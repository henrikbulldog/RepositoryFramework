using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Filters a sequence of entities using a predicate
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IFindWhereAsync<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Filters a collection of entities using a predicate
    /// </summary>
    /// <param name="where">Where predicate</param>
    /// <returns>Filtered collection of entities</returns>
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> where);
  }
}
