using RepositoryFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryFramework.EntityFramework
{
  /// <summary>
  /// Repository that uses Entity Framework Core 
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IEntityFrameworkRepository<TEntity> :
    IRepository<TEntity>,
    IExpandableRepository<TEntity>,
    ISortableRepository<TEntity>,
    IPageableRepository<TEntity>,
    IQueryableRepository<TEntity>,
    IUnitOfWorkRepository<TEntity>,
    IUnitOfWorkRepositoryAsync<TEntity>,
    IFindWhere<TEntity>,
    IFindWhereAsync<TEntity>
  where TEntity : class
  {
  }
}
