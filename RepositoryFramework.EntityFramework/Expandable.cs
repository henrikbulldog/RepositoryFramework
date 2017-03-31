using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.EntityFramework
{
  /// <summary>
  /// Can expand (or eager load) navigational properties and collections of an entity.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public class Expandable<TEntity> : Constrainable<TEntity>, IExpandable<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Gets a list of reference properties to include
    /// </summary>
    public List<string> Includes { get; private set; } = new List<string>();

    /// <summary>
    /// Include list of reference properties
    /// </summary>
    /// <param name="propertyPaths">Property paths</param>
    /// <returns>Current instance</returns>
    public IExpandable<TEntity> Include(List<string> propertyPaths)
    {
      if (propertyPaths == null)
      {
        throw new ArgumentNullException(nameof(propertyPaths));
      }

      foreach (var propertyPath in propertyPaths)
      {
        Include(propertyPath);
      }

      return this;
    }

    /// <summary>
    /// Include reference property
    /// </summary>
    /// <param name="propertyPath">Property path</param>
    /// <returns>Current instance</returns>
    public IExpandable<TEntity> Include(string propertyPath)
    {
      if (!string.IsNullOrEmpty(propertyPath))
      {
        var validatedPropertyPath = propertyPath;
        if (!ModelType.TryCheckPropertyPath(propertyPath, out validatedPropertyPath))
        {
          throw new ArgumentException(
            string.Format(
              "'{0}' is not a valid property path of '{1}'.",
              propertyPath,
              ModelType.FullName));
        }

        Includes.Add(validatedPropertyPath);
      }

      return this;
    }

    /// <summary>
    /// Include reference property
    /// </summary>
    /// <param name="property">Property expression</param>
    /// <returns>Current instance</returns>
    public IExpandable<TEntity> Include(Expression<Func<TEntity, object>> property)
    {
      var propertyPath = GetPropertyName(property);
      if (!ModelType.TryCheckPropertyPath(propertyPath, out propertyPath))
      {
        throw new ArgumentException(
            string.Format(
              "'{0}' is not a valid property path of '{1}'.",
              propertyPath,
              ModelType.FullName));
      }

      Includes.Add(propertyPath);
      return this;
    }
  }
}
