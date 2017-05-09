using RepositoryFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace RepositoryFramework.MongoDB
{
  public interface IMongoRepository<TEntity> : 
    ICreate<TEntity>,
    IUpdate<TEntity>,
    IDelete<TEntity>, 
    IGet<TEntity, object>
    where TEntity : class
  {
    IQueryResult<TEntity> Find(
      Expression<Func<TEntity, bool>> filter = null,
      QueryConstraints<TEntity> constraints = null);
  }
}
