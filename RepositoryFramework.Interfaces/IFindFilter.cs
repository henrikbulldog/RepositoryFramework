using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Filters a sequence of entities from a filter definition
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IFindFilter<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Filters a collection of entities from a filter definition
    /// </summary>
    /// <param name="filter">Filter definition</param>
    /// <returns>Filtered collection of entities</returns>
    IEnumerable<TEntity> Find(string filter);
  }
}
