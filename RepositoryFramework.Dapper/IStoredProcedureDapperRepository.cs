using RepositoryFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryFramework.Dapper
{
  /// <summary>
  /// A Dapper repository using stored procedures
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IStoredProcedureDapperRepository<TEntity> :
    IRepository<TEntity>,
    IParameterizedRepository<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Adds a parameter to queries
    /// </summary>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Parameter value</param>
    /// <returns>Current instance</returns>
    new IStoredProcedureDapperRepository<TEntity> SetParameter(string name, object value);

    /// <summary>
    /// Clears parameters
    /// </summary>
    /// <returns>Current instance</returns>
    new IStoredProcedureDapperRepository<TEntity> ClearParameters();
  }
}
