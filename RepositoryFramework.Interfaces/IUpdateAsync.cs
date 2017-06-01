using System.Threading.Tasks;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Updates an etity.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IUpdateAsync<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Task</returns>
    Task UpdateAsync(TEntity entity);
  }
}
