namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Gets an entity by id.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  /// <typeparam name="TFilter">Filter type</typeparam>
  public interface IGet<TEntity, in TFilter>
    where TEntity : class
  {
    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="filter">Filter</param>
    /// <returns>Entity</returns>
    TEntity GetById(TFilter filter);
  }
}
