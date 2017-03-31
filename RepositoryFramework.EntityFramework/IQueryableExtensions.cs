using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RepositoryFramework.EntityFramework
{
  /// <summary>
  /// Extensions for <see cref="ISortableExtensions"/>
  /// </summary>
  public static class IQueryableExtensions
  {
    /// <summary>
    /// Apply ordering to a LINQ query
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="source">Linq query</param>
    /// <param name="propertyName">Property to sort by</param>
    /// <returns>Ordered query</returns>
    public static IQueryable<TEntity> OrderBy<TEntity>(
      this IQueryable<TEntity> source,
      string propertyName)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      if (propertyName == null)
      {
        throw new ArgumentNullException(nameof(propertyName));
      }

      var type = typeof(TEntity);
      var property = type.GetProperty(propertyName);
      var parameter = Expression.Parameter(type, "p");
      var propertyAccess = Expression.MakeMemberAccess(parameter, property);
      var orderByExp = Expression.Lambda(propertyAccess, parameter);
      var resultExp = Expression.Call(
        typeof(Queryable),
        "OrderBy",
        new[] { type, property.PropertyType },
        source.Expression,
        Expression.Quote(orderByExp));
      return source.Provider.CreateQuery<TEntity>(resultExp);
    }

    /// <summary>
    /// Apply ordering to a LINQ query
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="source">Linq query</param>
    /// <param name="propertyName">Property to sort by</param>
    /// <returns>Ordered query</returns>
    public static IQueryable<TEntity> OrderByDescending<TEntity>(
      this IQueryable<TEntity> source,
      string propertyName)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      if (propertyName == null)
      {
        throw new ArgumentNullException(nameof(propertyName));
      }

      var type = typeof(TEntity);
      var property = type.GetProperty(propertyName);
      var parameter = Expression.Parameter(type, "p");
      var propertyAccess = Expression.MakeMemberAccess(parameter, property);
      var orderByExp = Expression.Lambda(propertyAccess, parameter);
      var resultExp = Expression.Call(
        typeof(Queryable),
        "OrderByDescending",
        new[] { type, property.PropertyType },
        source.Expression,
        Expression.Quote(orderByExp));
      return source.Provider.CreateQuery<TEntity>(resultExp);
    }
  }
}
