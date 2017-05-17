using System.Collections.Generic;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Deletes an entity.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IDeleteMany<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Delete a list of existing entities
    /// </summary>
    /// <param name="entities">Entity list</param>
    void DeleteMany(IEnumerable<TEntity> entities);
  }
}
