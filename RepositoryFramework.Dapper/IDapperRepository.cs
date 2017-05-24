using RepositoryFramework.Interfaces;

namespace RepositoryFramework.Dapper
{
  /// <summary>
  /// Repository that uses the Dapper micro-ORM framework, see https://github.com/StackExchange/Dapper
  /// </summary>
  /// <typeparam name="TEntity"></typeparam>
  public interface IDapperRepository<TEntity> :
    IRepository<TEntity>,
    ISortableRepository<TEntity>,
    IPageableRepository<TEntity>
    where TEntity : class
  {
  }
}
