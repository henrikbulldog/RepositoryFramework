using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Finds a list of entites.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IFindAsync<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Get a list of entities
    /// </summary>
    /// <returns>Query result</returns>
    Task<IEnumerable<TEntity>> FindAsync();
  }
}
