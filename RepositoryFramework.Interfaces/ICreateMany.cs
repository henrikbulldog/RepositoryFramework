using System.Collections.Generic;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Creates a list of new entities.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface ICreateMany<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Create a list of new entities
    /// </summary>
    /// <param name="entities">List of entities</param>
    void CreateMany(IEnumerable<TEntity> entities);
  }
}