using RepositoryFramework.Interfaces;
using System;
using System.Linq;

namespace RepositoryFramework.EntityFramework
{
	/// <summary>
	/// Extensions for <see cref="ISortableExtensions"/>
	/// </summary>
	public static class ISortableExtensions
    {
		public static IQueryable<T> AddSorting<T>(this ISortable<T> instance,
			IQueryable<T> query) where T : class
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			if (query == null) throw new ArgumentNullException(nameof(query));

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
