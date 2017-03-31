using System;
using System.Linq;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.EntityFramework
{
  /// <summary>
  /// Extensions for <see cref="IPageableExtensions"/>
  /// </summary>
  public static class IPageableExtensions
  {
    /// <summary>
    /// Add paging to a queryable expression
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="instance">Pageable instance</param>
    /// <param name="query">Queryable expression</param>
    /// <returns>Queryable expression weith paging</returns>
    public static IQueryable<TEntity> AddPaging<TEntity>(
      this IPageable<TEntity> instance,
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

      if (instance.PageNumber >= 1)
      {
        query = query.Skip((instance.PageNumber - 1) * instance.PageSize)
          .Take(instance.PageSize);
      }

      return query;
    }
  }
}
