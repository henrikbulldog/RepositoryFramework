namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Creates, updates and deletes entities.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IRepository<TEntity>
    : ICreate<TEntity>,
    IUpdate<TEntity>,
    IDelete<TEntity>
    where TEntity : class
  {
  }
}