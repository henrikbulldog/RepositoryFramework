using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.EntityFramework
{
  /// <summary>
  /// Typed constraints
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public class QueryConstraints<TEntity> : IQueryConstraints<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryConstraints{TEntity}"/> class.
    /// </summary>
    public QueryConstraints()
    {
      ModelType = typeof(TEntity);
      Expandable = new Expandable<TEntity>();
      Pageable = new Pageable<TEntity>();
      Sortable = new Sortable<TEntity>();
    }

    /// <summary>
    /// Gets an Expandable object
    /// </summary>
    public IExpandable<TEntity> Expandable { get; private set; }

    /// <summary>
    /// Gets a Pageable object
    /// </summary>
    public IPageable<TEntity> Pageable { get; private set; }

    /// <summary>
    /// Gets a Sortable object
    /// </summary>
    public ISortable<TEntity> Sortable { get; private set; }

    /// <summary>
    /// Gets or sets model which will be queried
    /// </summary>
    protected Type ModelType { get; set; }

    /// <summary>
    /// Use paging
    /// </summary>
    /// <param name="pageNumber">Page to get (one based index).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>Current instance</returns>
    public IQueryConstraints<TEntity> Page(int pageNumber, int pageSize)
    {
      Pageable.Page(pageNumber, pageSize);
      return this;
    }

    /// <summary>
    /// Include reference property
    /// </summary>
    /// <param name="propertyPath">Property path</param>
    /// <returns>Current instance</returns>
    public IQueryConstraints<TEntity> Include(string propertyPath)
    {
      Expandable.Include(propertyPath);
      return this;
    }

    /// <summary>
    /// Include reference properties
    /// </summary>
    /// <param name="propertyPaths">Property paths</param>
    /// <returns>Current instance</returns>
    public IQueryConstraints<TEntity> Include(List<string> propertyPaths)
    {
      Expandable.Include(propertyPaths);
      return this;
    }

    /// <summary>
    /// Sort ascending by a property
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    public IQueryConstraints<TEntity> SortBy(string propertyName)
    {
      Sortable.SortBy(propertyName);
      return this;
    }

    /// <summary>
    /// Sort descending by a property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    public IQueryConstraints<TEntity> SortByDescending(string propertyName)
    {
      Sortable.SortByDescending(propertyName);
      return this;
    }

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>Current instance</returns>
    public IQueryConstraints<TEntity> SortBy(Expression<Func<TEntity, object>> property)
    {
      Sortable.SortBy(property);
      return this;
    }

    /// <summary>
    /// Property to sort by (descending)
    /// </summary>
    /// <param name="property">The property</param>
    /// <returns>Current instance</returns>
    public IQueryConstraints<TEntity> SortByDescending(Expression<Func<TEntity, object>> property)
    {
      Sortable.SortByDescending(property);
      return this;
    }

    /// <summary>
    /// Add expansion expression
    /// </summary>
    /// <param name="property">Property expression</param>
    /// <returns>Current instance</returns>
    public IQueryConstraints<TEntity> Include(Expression<Func<TEntity, object>> property)
    {
      Expandable.Include(property);
      return this;
    }
  }
}
