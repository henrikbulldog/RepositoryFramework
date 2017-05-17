using System;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Persists changes made to a repository and detaches all entities from the repository
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IUnitOfWorkRepository<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Persist all changes to the data storage
    /// <returns>Current instance</returns>
    /// </summary>
    /// <returns>Current instance</returns>
    IRepository<TEntity> SaveChanges();

    /// <summary>
    /// Detach all entites from the repository
    /// <returns>Current instance</returns>
    /// </summary>
    /// <returns>Current instance</returns>
    IRepository<TEntity> DetachAll();
  }
}
