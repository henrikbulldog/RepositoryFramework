using System.Threading.Tasks;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Deletes an entity.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IDeleteAsync<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Task</returns>
    Task DeleteAsync(TEntity entity);
  }
}
