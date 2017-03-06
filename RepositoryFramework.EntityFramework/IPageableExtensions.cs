using System;
using System.Linq;

namespace RepositoryFramework.EntityFramework
{
		/// <summary>
		/// Extensions for <see cref="IPageableExtensions"/>
		/// </summary>
    public static class IPageableExtensions
    {
		public static IQueryable<T> AddPaging<T>(this IPageable<T> instance, IQueryable<T> query) where T : class
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			if (query == null) throw new ArgumentNullException(nameof(query));

			if (instance.PageNumber >= 1)
			{
				query = query.Skip((instance.PageNumber - 1) * instance.PageSize)
					.Take(instance.PageSize);
			}

			return query;
		}

	}
}
