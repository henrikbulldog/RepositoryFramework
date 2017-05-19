using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Filters a sequence of entities
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IFindFilterAsync<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Filters a collection of entities
    /// </summary>
    /// <param name="filter">Filter</param>
    /// <returns>Filtered collection of entities</returns>
    Task<IEnumerable<TEntity>> FindAsync(string filter);
  }
}
