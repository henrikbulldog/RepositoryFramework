using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Extensions for <see cref="IQueryable"/>
  /// </summary>
  public static class IQueryableExtensions
  {
    /// <summary>
    /// Sorts a queryable collection
    /// </summary>
    /// <param name="query">Queryable collection</param>
    /// <param name="sortable">Sortable repository</param>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <returns>Sorted queryable collection</returns>
    public static IQueryable<TEntity> Sort<TEntity>(
      this IQueryable<TEntity> query,
      ISortableRepository<TEntity> sortable)
      where TEntity : class
    {
      if (query == null)
      {
        throw new ArgumentNullException(nameof(query));
      }

      if (sortable == null)
      {
        throw new ArgumentNullException(nameof(sortable));
      }

      if (string.IsNullOrEmpty(sortable.SortPropertyName))
      {
        return query;
      }

      if (sortable.SortOrder == SortOrder.Unspecified)
      {
        return query;
      }

      var property = typeof(TEntity).GetProperty(sortable.SortPropertyName);
      var parameter = Expression.Parameter(typeof(TEntity), "p");
      var propertyAccess = Expression.MakeMemberAccess(parameter, property);
      var orderByExp = Expression.Lambda(propertyAccess, parameter);
      var resultExp = Expression.Call(
        typeof(Queryable),
        sortable.SortOrder == SortOrder.Ascending ? "OrderBy" : "OrderByDescending",
        new[] { typeof(TEntity), property.PropertyType },
        query.Expression,
        Expression.Quote(orderByExp));
      return query.Provider.CreateQuery<TEntity>(resultExp);
    }

    /// <summary>
    /// Page a queryable collection
    /// </summary>
    /// <param name="query">Queryable collection</param>
    /// <param name="pageable">Pageable repository</param>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <returns>Paged queryable collection</returns>
    public static IQueryable<TEntity> Page<TEntity>(
      this IQueryable<TEntity> query,
      IPageableRepository<TEntity> pageable)
      where TEntity : class
    {
      if (query == null)
      {
        throw new ArgumentNullException(nameof(query));
      }

      if (pageable == null)
      {
        throw new ArgumentNullException(nameof(pageable));
      }

      if (pageable.PageSize > 0)
      {
        if (pageable.PageNumber >= 1)
        {
          query = query.Skip((pageable.PageNumber - 1) * pageable.PageSize);
        }

        query = query.Take(pageable.PageSize);
      }

      return query;
    }
  }
}
