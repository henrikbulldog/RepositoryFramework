using RepositoryFramework.Interfaces;
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
		/// Execute LINQ query and fill a result.
		/// </summary>
		/// <typeparam name="T">Model type</typeparam>
		/// <param name="instance">The instance</param>
		/// <param name="constraints">The constraints</param>
		/// <returns>Search reuslt</returns>
		public static IQueryResult<T> ToSearchResult<T>(this IQueryable<T> instance,
			QueryConstraints<T> constraints)
				where T : class
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			if (constraints == null) throw new ArgumentNullException(nameof(constraints));

			var totalCount = instance.Count();
			var limitedQuery = constraints.ApplyTo(instance);
			return new QueryResult<T>(limitedQuery, totalCount);
		}

		/// <summary>
		/// Execute LINQ query and fill a result.
		/// </summary>
		/// <typeparam name="TFrom">Database Model type</typeparam>
		/// <typeparam name="TTo">Query result item type</typeparam>
		/// <param name="instance">The instance</param>
		/// <param name="constraints">The constraints</param>
		/// <param name="converter">Method used to convert the result</param>
		/// <returns>Search reuslt</returns>
		public static IQueryResult<TTo> ToSearchResult<TFrom, TTo>(this IQueryable<TFrom> instance,
			QueryConstraints<TFrom> constraints,
			Func<TFrom, TTo> converter)
			where TFrom : class where TTo : class
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			if (constraints == null) throw new ArgumentNullException(nameof(constraints));

			var totalCount = instance.Count();
			var limitedQuery = constraints.ApplyTo(instance);
			return new QueryResult<TTo>(limitedQuery.Select(converter), totalCount);
		}

		/// <summary>
		/// Apply ordering to a LINQ query
		/// </summary>
		/// <typeparam name="T">Model type</typeparam>
		/// <param name="source">Linq query</param>
		/// <param name="propertyName">Property to sort by</param>
		/// <param name="values">DUNNO?</param>
		/// <returns>Ordered query</returns>
		public static IQueryable<T> OrderBy<T>(this IQueryable<T> source,
			string propertyName, params object[] values)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

			var type = typeof(T);
			var property = type.GetProperty(propertyName);
			var parameter = Expression.Parameter(type, "p");
			var propertyAccess = Expression.MakeMemberAccess(parameter, property);
			var orderByExp = Expression.Lambda(propertyAccess, parameter);
			var resultExp = Expression.Call(typeof(Queryable), "OrderBy", new[] { type, property.PropertyType },
																			source.Expression, Expression.Quote(orderByExp));
			return source.Provider.CreateQuery<T>(resultExp);
		}

		/// <summary>
		/// Apply ordering to a LINQ query
		/// </summary>
		/// <typeparam name="T">Model type</typeparam>
		/// <param name="source">Linq query</param>
		/// <param name="propertyName">Property to sort by</param>
		/// <param name="values">DUNNO?</param>
		/// <returns>Ordered query</returns>
		public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> source,
			string propertyName, params object[] values)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

			var type = typeof(T);
			var property = type.GetProperty(propertyName);
			var parameter = Expression.Parameter(type, "p");
			var propertyAccess = Expression.MakeMemberAccess(parameter, property);
			var orderByExp = Expression.Lambda(propertyAccess, parameter);
			var resultExp = Expression.Call(typeof(Queryable),
				"OrderByDescending", new[] { type, property.PropertyType },
				source.Expression, Expression.Quote(orderByExp));
			return source.Provider.CreateQuery<T>(resultExp);
		}
	}
}
