using System;
using System.Linq.Expressions;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Sorts a result sets
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface ISortableRepository<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Gets the kind of sort order
    /// </summary>
    SortOrder SortOrder { get; }

    /// <summary>
    /// Gets property name for the property to sort by.
    /// </summary>
    string SortPropertyName { get; }

    /// <summary>
    /// Sort ascending by a property
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    IRepository<TEntity> SortBy(string propertyName);

    /// <summary>
    /// Sort descending by a property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    IRepository<TEntity> SortByDescending(string propertyName);

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>Current instance</returns>
    IRepository<TEntity> SortBy(Expression<Func<TEntity, object>> property);

    /// <summary>
    /// Property to sort by (descending)
    /// </summary>
    /// <param name="property">The property</param>
    /// <returns>Current instance</returns>
    IRepository<TEntity> SortByDescending(Expression<Func<TEntity, object>> property);

    /// <summary>
    /// Clear sorting
    /// </summary>
    /// <returns>Current instance</returns>
    IRepository<TEntity> ClearSorting();
  }
}