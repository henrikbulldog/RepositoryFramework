using System;
using System.Linq;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.EntityFramework
{
  /// <summary>
  /// Extensions for <see cref="ISortableExtensions"/>
  /// </summary>
  public static class ISortableExtensions
  {
    /// <summary>
    /// Add sorting constraint to a queryable expression
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="instance">Sortable instance</param>
    /// <param name="query">Queryable expression</param>
    /// <returns>Queryable expression with sortable constraint</returns>
    public static IQueryable<TEntity> AddSorting<TEntity>(
      this ISortable<TEntity> instance,
      IQueryable<TEntity> query)
      where TEntity : class
    {
      if (instance == null)
      {
        throw new ArgumentNullException(nameof(instance));
      }

      if (query == null)
      {
        throw new ArgumentNullException(nameof(query));
      }

      if (!string.IsNullOrEmpty(instance.SortPropertyName))
      {
        query = instance.SortOrder == SortOrder.Ascending
                    ? query.OrderBy(instance.SortPropertyName)
                    : query.OrderByDescending(instance.SortPropertyName);
      }

      return query;
    }
  }
}
