using System;
using System.Threading.Tasks;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Asynchronously persists changes made to a repository and detaches all entities from the repository
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IUnitOfWorkRepositoryAsync<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Persists all changes to the data storage
    /// <returns>Current instance</returns>
    /// </summary>
    /// <returns>Current instance</returns>
    Task<IRepository<TEntity>> SaveChangesAsync();

    /// <summary>
    /// Detaches all entites from the repository
    /// <returns>Current instance</returns>
    /// </summary>
    /// <returns>Current instance</returns>
    Task<IRepository<TEntity>> DetachAllAsync();
  }
}
