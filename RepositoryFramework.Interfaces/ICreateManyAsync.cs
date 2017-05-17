using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Creates a list of new entities.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface ICreateManyAsync<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Create a list of new entities
    /// </summary>
    /// <param name="entities">List of entities</param>
    Task CreateManyAsync(IEnumerable<TEntity> entities);
  }
}