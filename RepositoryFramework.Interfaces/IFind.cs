namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Finds a list of entites.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IFind<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Get a list of entities
    /// </summary>
    /// <returns>Query result</returns>
    IQueryResult<TEntity> Find();
  }
}
