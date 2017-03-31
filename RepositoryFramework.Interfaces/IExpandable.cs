using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Can expand (or eager load) navigational properties and collections of an entity.
  /// </summary>
  /// <typeparam name="TEntity">Entity</typeparam>
  public interface IExpandable<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Gets a list of reference properties to include
    /// </summary>
    List<string> Includes { get; }

    /// <summary>
    /// Include reference property
    /// </summary>
    /// <param name="propertyPath">Property path</param>
    /// <returns>Fluent self reference</returns>
    IExpandable<TEntity> Include(string propertyPath);

    /// <summary>
    /// Include reference property
    /// </summary>
    /// <param name="property">Property expression</param>
    /// <returns>Current instance</returns>
    IExpandable<TEntity> Include(Expression<Func<TEntity, object>> property);

    /// <summary>
    /// Include list of reference properties
    /// </summary>
    /// <param name="propertyPaths">Property paths</param>
    /// <returns>Current instance</returns>
    IExpandable<TEntity> Include(List<string> propertyPaths);
  }
}