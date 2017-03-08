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
