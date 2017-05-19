using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Loads data for referenced objects
  /// </summary>
  /// <typeparam name="TEntity">Type of entity</typeparam>
  public interface IExpandableRepository<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Gets a list of reference properties to include
    /// </summary>
    List<string> Includes { get; }

    /// <summary>
    /// Include referenced properties
    /// </summary>
    /// <param name="propertyPaths">Comma-separated list of property paths</param>
    /// <returns>Current instance</returns>
    IRepository<TEntity> Include(string propertyPaths);

    /// <summary>
    /// Include referenced property
    /// </summary>
    /// <param name="property">Property expression</param>
    /// <returns>Current instance</returns>
    IRepository<TEntity> Include(Expression<Func<TEntity, object>> property);

    /// <summary>
    /// Clear includes
    /// </summary>
    /// <returns>Current instance</returns>
    IRepository<TEntity> ClearIncludes();
  }
}