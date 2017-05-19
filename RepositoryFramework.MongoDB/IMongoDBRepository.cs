using RepositoryFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryFramework.MongoDB
{
  /// <summary>
  /// Repository that uses the MongoDB document database, see https://docs.mongodb.com/
  /// </summary>
  /// <typeparam name="TEntity"></typeparam>
  public interface IMongoDBRepository<TEntity> :
    IRepository<TEntity>,
    ISortableRepository<TEntity>,
    IPageableRepository<TEntity>,
    IQueryableRepository<TEntity>,
    IFindWhere<TEntity>,
    IFindWhereAsync<TEntity>,
    IFindFilter<TEntity>,
    IFindFilterAsync<TEntity>
    where TEntity : class
  {
  }
}
