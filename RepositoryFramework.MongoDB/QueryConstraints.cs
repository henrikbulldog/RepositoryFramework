using RepositoryFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RepositoryFramework.MongoDB
{
  public class QueryConstraints<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryConstraints{TEntity}"/> class
    /// </summary>
    public QueryConstraints()
    {
      ModelType = typeof(TEntity);
      PageSize = 50;
      PageNumber = 1;
    }
    
    /// <summary>
    /// Gets or sets the model which will be queried.
    /// </summary>
    protected Type ModelType { get; set; }

    /// <summary>
    /// Get property name from expression
    /// </summary>
    /// <param name="exp">Property expression</param>
    /// <returns>Property name</returns>
    protected static string GetPropertyName(Expression<Func<TEntity, object>> exp)
    {
      var body = exp.Body as MemberExpression;

      if (body != null)
      {
        return body.Member.Name;
      }

      var ubody = (UnaryExpression)exp.Body;
      body = ubody.Operand as MemberExpression;

      return body?.Member.Name ?? string.Empty;
    }

    /// <summary>
    /// Gets the kind of sort order
    /// </summary>
    public SortOrder SortOrder { get; private set; }

    /// <summary>
    /// Gets property name for the property to sort by.
    /// </summary>
    public string SortPropertyName { get; private set; }

    /// <summary>
    /// Sort ascending by a property
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    public QueryConstraints<TEntity> SortBy(string propertyName)
    {
      if (propertyName == null)
      {
        throw new ArgumentNullException(nameof(propertyName));
      }

      ValidatePropertyName(propertyName, out propertyName);

      SortOrder = SortOrder.Ascending;
      SortPropertyName = propertyName;
      return this;
    }

    /// <summary>
    /// Sort descending by a property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    public QueryConstraints<TEntity> SortByDescending(string propertyName)
    {
      if (propertyName == null)
      {
        throw new ArgumentNullException(nameof(propertyName));
      }

      ValidatePropertyName(propertyName, out propertyName);

      SortOrder = SortOrder.Descending;
      SortPropertyName = propertyName;
      return this;
    }

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>Current instance</returns>
    public QueryConstraints<TEntity> SortBy(Expression<Func<TEntity, object>> property)
    {
      if (property == null)
      {
        throw new ArgumentNullException(nameof(property));
      }

      var name = GetPropertyName(property);
      SortBy(name);
      return this;
    }

    /// <summary>
    /// Property to sort by (descending)
    /// </summary>
    /// <param name="property">The property</param>
    /// <returns>Current instance</returns>
    public QueryConstraints<TEntity> SortByDescending(Expression<Func<TEntity, object>> property)
    {
      if (property == null)
      {
        throw new ArgumentNullException(nameof(property));
      }

      var name = GetPropertyName(property);
      SortByDescending(name);
      return this;
    }

    /// <summary>
    /// Make sure that the property exists in the model.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="validatedName">Validated name</param>
    protected virtual void ValidatePropertyName(string name, out string validatedName)
    {
      validatedName = name;
      if (name == null)
      {
        throw new ArgumentNullException(nameof(name));
      }

      if (!ModelType.TryCheckPropertyName(name, out validatedName))
      {
        throw new ArgumentException(
          string.Format(
            "'{0}' is not a public property of '{1}'.",
            name,
            ModelType.FullName));
      }
    }

    /// <summary>
    /// Gets number of items per page (when paging is used)
    /// </summary>
    public int PageSize { get; private set; }

    /// <summary>
    /// Gets page number (one based index)
    /// </summary>
    public int PageNumber { get; private set; }

    /// <summary>
    /// Use paging
    /// </summary>
    /// <param name="pageNumber">Page to get (one based index).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>Current instance</returns>
    public QueryConstraints<TEntity> Page(int pageNumber, int pageSize)
    {
      if (pageNumber < 1 || pageNumber > 1000)
      {
        throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be between 1 and 1000.");
      }

      if (pageSize < 1 || pageNumber > 1000)
      {
        throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 1000.");
      }

      PageSize = pageSize;
      PageNumber = pageNumber;
      return this;
    }
  }
}