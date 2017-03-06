using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
		/// <typeparam name="T">Model type</typeparam>
		/// <param name="instance">contraints instance</param>
		/// <param name="query">LINQ queryable</param>
		public static IQueryable<T> AddExpansion<T>(this IExpandable<T> instance,
			IQueryable<T> query) where T : class
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			if (query == null) throw new ArgumentNullException(nameof(query));

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
