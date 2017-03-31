namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Finds a list of entites using an object with filtering information
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  /// <typeparam name="TFilter">Filter type</typeparam>
  public interface IFindFilter<TEntity, TFilter>
    where TEntity : class
  {
    /// <summary>
    /// Get a list of entites using an object with filtering information
    /// </summary>
    /// <param name="filter">Filter object</param>
    /// <returns>Query result</returns>
    IQueryResult<TEntity> Find(TFilter filter);
  }
}
