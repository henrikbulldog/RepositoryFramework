using RepositoryFramework.Interfaces;
using System;
using System.Linq;

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
		/// <typeparam name="T">Model type</typeparam>
		/// <param name="instance">contraints instance</param>
		/// <param name="query">LINQ queryable</param>
		/// <returns>Modified query</returns>
		public static IQueryable<T> ApplyTo<T>(this IQueryConstraints<T> instance, 
			IQueryable<T> query) where T : class
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			if (query == null) throw new ArgumentNullException(nameof(query));

			query = instance.Sortable.AddSorting(query);
			query = instance.Pageable.AddPaging(query);
			query = instance.Expandable.AddExpansion(query);

			return query;
		}

	}
}
