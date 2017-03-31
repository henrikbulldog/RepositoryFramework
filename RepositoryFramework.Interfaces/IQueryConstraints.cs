using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Query constraints for expansion, paging and sorting.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IQueryConstraints<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Gets a Sortable object
    /// </summary>
    ISortable<TEntity> Sortable { get; }

    /// <summary>
    /// Gets a Pageable object
    /// </summary>
    IPageable<TEntity> Pageable { get; }

    /// <summary>
    /// Gets an Expandable object
    /// </summary>
    IExpandable<TEntity> Expandable { get; }

    /// <summary>
    /// Include reference property
    /// </summary>
    /// <param name="propertyPath">Property path</param>
    /// <returns>Current instance</returns>
    IQueryConstraints<TEntity> Include(string propertyPath);

    /// <summary>
    /// Include reference properties
    /// </summary>
    /// <param name="propertyPaths">Property paths</param>
    /// <returns>Current instance</returns>
    IQueryConstraints<TEntity> Include(List<string> propertyPaths);

    /// <summary>
    /// Include reference property
    /// </summary>
    /// <param name="property">Property expression</param>
    /// <returns>Current instance</returns>
    IQueryConstraints<TEntity> Include(Expression<Func<TEntity, object>> property);

    /// <summary>
    /// Use paging
    /// </summary>
    /// <param name="pageNumber">Page to get (one based index).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>Current instance</returns>
    IQueryConstraints<TEntity> Page(int pageNumber, int pageSize);

    /// <summary>
    /// Sort ascending by a property
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    IQueryConstraints<TEntity> SortBy(string propertyName);

    /// <summary>
    /// Sort descending by a property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    IQueryConstraints<TEntity> SortByDescending(string propertyName);

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>Current instance</returns>
    IQueryConstraints<TEntity> SortBy(Expression<Func<TEntity, object>> property);

    /// <summary>
    /// Property to sort by (descending)
    /// </summary>
    /// <param name="property">The property</param>
    /// <returns>Current instance</returns>
    IQueryConstraints<TEntity> SortByDescending(Expression<Func<TEntity, object>> property);
  }
}