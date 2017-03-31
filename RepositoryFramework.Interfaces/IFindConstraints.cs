namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Finds a list of entities using paging and sorting constraints
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IFindConstraints<TEntity> : IFind<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Get a list of entities using paging and sorting constraints
    /// </summary>
    /// <param name="constraints">Paging and sorting constraints</param>
    /// <returns>Query result</returns>
    IQueryResult<TEntity> Find(IQueryConstraints<TEntity> constraints);
  }
}
