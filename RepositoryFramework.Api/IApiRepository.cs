using RepositoryFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryFramework.Api
{
  /// <summary>
  /// Repository abstraction of a ReSTful API
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IApiRepository<TEntity> :
  IRepository<TEntity>,
  IParameterizedRepository<TEntity>
  where TEntity : class
  {
    /// <summary>
    /// Gets configuration object
    /// </summary>
    ApiConfiguration Configuration { get; }
  }
}
