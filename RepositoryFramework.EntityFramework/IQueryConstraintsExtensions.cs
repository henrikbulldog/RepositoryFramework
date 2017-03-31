using System;
using System.Linq;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.EntityFramework
{
  /// <summary>
  /// Extensions for <see cref="IQueryConstraintsExtensions"/>
  /// </summary>
  public static class IQueryConstraintsExtensions
  {
    /// <summary>
    /// Apply the query information to a LINQ statement
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="instance">Query contraints instance</param>
    /// <param name="query">Queryable expression</param>
    /// <returns>Queryable expression with constraints</returns>
    public static IQueryable<TEntity> ApplyTo<TEntity>(
      this IQueryConstraints<TEntity> instance,
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

      query = instance.Sortable.AddSorting(query);
      query = instance.Pageable.AddPaging(query);
      query = instance.Expandable.AddExpansion(query);

      return query;
    }
  }
}
