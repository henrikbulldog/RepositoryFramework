using System;
using System.Linq.Expressions;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.Api
{
  /// <summary>
  /// IRepository extension methods
  /// </summary>
  public static class IRepositoryExtensions
  {
    /// <summary>
    /// Adds a parameter to queries
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Parameter value</param>
    /// <returns>List of items</returns>
    public static IRepository<TEntity> SetParameter<TEntity>(
      this IRepository<TEntity> instance,
      string name,
      object value)
      where TEntity : class
    {
      var apiRepository = instance as ApiRepository<TEntity>;
      if (apiRepository == null)
      {
        throw new NotImplementedException();
      }

      apiRepository.SetParameter(name, value);
      return instance;
    }

    /// <summary>
    /// Gets parameter value
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="name">Parameter name</param>
    /// <returns>Parameter value</returns>
    public static object GetParameter<TEntity>(
      this IRepository<TEntity> instance,
      string name)
      where TEntity : class
    {
      var apiRepository = instance as ApiRepository<TEntity>;
      if (apiRepository == null)
      {
        throw new NotImplementedException();
      }

      return apiRepository.GetParameter(name);
    }

    /// <summary>
    /// Clears parameters
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> ClearParameters<TEntity>(
      this IRepository<TEntity> instance)
      where TEntity : class
    {
      var apiRepository = instance as ApiRepository<TEntity>;
      if (apiRepository == null)
      {
        throw new NotImplementedException();
      }

      return apiRepository.ClearParameters();
    }
  }
}
