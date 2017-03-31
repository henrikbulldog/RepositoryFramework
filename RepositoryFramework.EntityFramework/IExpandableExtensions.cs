using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.EntityFramework
{
    /// <summary>
    /// Extensions for <see cref="IExpandableExtensions"/>
    /// </summary>
    public static class IExpandableExtensions
    {
    /// <summary>
    /// Apply expansion to LINQ statement
    /// </summary>
    /// <typeparam name="TEntity">Model type</typeparam>
    /// <param name="instance">Expandable instance</param>
    /// <param name="query">Queryable expression</param>
    /// <returns>Queryable expression with expansion</returns>
    public static IQueryable<TEntity> AddExpansion<TEntity>(
      this IExpandable<TEntity> instance,
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

      if (instance.Includes != null)
      {
        foreach (var propertyName in instance.Includes)
        {
          query = query.Include(propertyName);
        }
      }

      return query;
    }
  }
}
