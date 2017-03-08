using RepositoryFramework.Interfaces;
using System;

namespace RepositoryFramework.EntityFramework
{
	internal class Pageable<TEntity> : IPageable<TEntity> where TEntity : class
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Pageable{TEntity}"/> class.
		/// </summary>
		/// <remarks>Will per default return the first 50 items</remarks>
		public Pageable()
		{
			PageSize = 50;
			PageNumber = 1;
		}
		/// <summary>
		/// Gets number of items per page (when paging is used)
		/// </summary>
		public int PageSize { get; private set; }

		/// <summary>
		/// Gets page number (one based index)
		/// </summary>
		public int PageNumber { get; private set; }

		/// <summary>
		/// Use paging
		/// </summary>
		/// <param name="pageNumber">Page to get (one based index).</param>
		/// <param name="pageSize">Number of items per page.</param>
		/// <returns>Current instance</returns>
		public IPageable<TEntity> Page(int pageNumber, int pageSize)
		{
			if (pageNumber < 1 || pageNumber > 1000)
				throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be between 1 and 1000.");

			if (pageSize < 1 || pageNumber > 1000)
				throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 1000.");

			PageSize = pageSize;
			PageNumber = pageNumber;
			return this;
		}

	}
}