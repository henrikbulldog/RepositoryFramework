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
    /// Gets a value indicating whether changes are committed automatically
    /// </summary>
    /// <remarks>If false, SaveChanges() msut be called before changes are committed</remarks>
    bool AutoCommit { get; }

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
