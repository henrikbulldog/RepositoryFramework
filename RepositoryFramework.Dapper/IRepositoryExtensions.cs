using System;
using System.Linq.Expressions;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.Dapper
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
      var dapperRepository = instance as StoredProcedureDapperRepository<TEntity>;
      if (dapperRepository == null)
      {
        throw new NotImplementedException();
      }

      dapperRepository.SetParameter(name, value);
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
      var dapperRepository = instance as StoredProcedureDapperRepository<TEntity>;
      if (dapperRepository == null)
      {
        throw new NotImplementedException();
      }

      return dapperRepository.GetParameter(name);
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
      var dapperRepository = instance as StoredProcedureDapperRepository<TEntity>;
      if (dapperRepository == null)
      {
        throw new NotImplementedException();
      }

      return dapperRepository.ClearParameters();
    }

    /// <summary>
    /// Use paging
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="pageNumber">Page to get (one based index).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> Page<TEntity>(
      this IRepository<TEntity> instance,
      int pageNumber,
      int pageSize)
      where TEntity : class
    {
      var dapperRepository = instance as DapperRepository<TEntity>;
      if (dapperRepository == null)
      {
        throw new NotImplementedException();
      }

      dapperRepository.Page(pageNumber, pageSize);
      return instance;
    }

    /// <summary>
    /// Clear paging
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> ClearPaging<TEntity>(
      this IRepository<TEntity> instance)
      where TEntity : class
    {
      var dapperRepository = instance as DapperRepository<TEntity>;
      if (dapperRepository == null)
      {
        throw new NotImplementedException();
      }

      dapperRepository.ClearPaging();
      return instance;
    }

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="property">The property.</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> SortBy<TEntity>(
      this IRepository<TEntity> instance,
      Expression<Func<TEntity, object>> property)
      where TEntity : class
    {
      var dapperRepository = instance as DapperRepository<TEntity>;
      if (dapperRepository == null)
      {
        throw new NotImplementedException();
      }

      dapperRepository.SortBy(property);
      return instance;
    }

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> SortBy<TEntity>(
      this IRepository<TEntity> instance,
      string propertyName)
      where TEntity : class
    {
      var dapperRepository = instance as DapperRepository<TEntity>;
      if (dapperRepository == null)
      {
        throw new NotImplementedException();
      }

      dapperRepository.SortBy(propertyName);
      return instance;
    }

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="property">The property.</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> SortByDescending<TEntity>(
      this IRepository<TEntity> instance,
      Expression<Func<TEntity, object>> property)
      where TEntity : class
    {
      var dapperRepository = instance as DapperRepository<TEntity>;
      if (dapperRepository == null)
      {
        throw new NotImplementedException();
      }

      dapperRepository.SortByDescending(property);
      return instance;
    }

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> SortByDescending<TEntity>(
      this IRepository<TEntity> instance,
      string propertyName)
      where TEntity : class
    {
      var dapperRepository = instance as DapperRepository<TEntity>;
      if (dapperRepository == null)
      {
        throw new NotImplementedException();
      }

      dapperRepository.SortByDescending(propertyName);
      return instance;
    }
  }
}
