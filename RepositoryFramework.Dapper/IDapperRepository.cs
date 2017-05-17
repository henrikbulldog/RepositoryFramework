using RepositoryFramework.Interfaces;

namespace RepositoryFramework.Dapper
{
  /// <summary>
  /// Repository that uses the Dapper micro-ORM framework, see https://github.com/StackExchange/Dapper
  /// </summary>
  /// <typeparam name="TEnitity"></typeparam>
  public interface IDapperRepository<TEnitity> :
    IRepository<TEnitity>,
    ISortableRepository<TEnitity>,
    IPageableRepository<TEnitity>
    where TEnitity : class
  {
  }
}
