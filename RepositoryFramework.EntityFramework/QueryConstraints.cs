using RepositoryFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RepositoryFramework.EntityFramework
{
	/// <summary>
	/// Typed constraints
	/// </summary>
	/// <typeparam name="TEntity">Model to query</typeparam>
	public class QueryConstraints<TEntity> : IQueryConstraints<TEntity> where TEntity : class
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="QueryConstraints{TEntity}"/> class.
		/// </summary>
		public QueryConstraints()
		{
			ModelType = typeof(TEntity);
			Expandable = new Expandable<TEntity>();
			Pageable = new Pageable<TEntity>();
			Sortable = new Sortable<TEntity>();
		}

		/// <summary>
		/// Gets model which will be queried.
		/// </summary>
		protected Type ModelType { get; set; }

		/// <summary>
		/// Gets start record (in the data source)
		/// </summary>
		/// <remarks>Calculated with the help of PageNumber and PageSize.</remarks>
		/// 
		public int StartRecord
		{
			get
			{
				if (Pageable.PageNumber <= 1)
					return 0;

				return (Pageable.PageNumber - 1) * (Pageable.PageSize);
			}
		}


		public IExpandable<TEntity> Expandable { get; private set; }
		public IPageable<TEntity> Pageable { get; private set; }
		public ISortable<TEntity> Sortable { get; private set; }


		/// <summary>
		/// Use paging
		/// </summary>
		/// <param name="pageNumber">Page to get (one based index).</param>
		/// <param name="pageSize">Number of items per page.</param>
		public IQueryConstraints<TEntity> Page(int pageNumber, int pageSize)
		{
			Pageable.Page(pageNumber, pageSize);
			return this;
		}

		/// <summary>
		/// Include reference property
		/// </summary>
		/// <param name="propertyPath"></param>
		public IQueryConstraints<TEntity> Include(string propertyPath)
		{
			Expandable.Include(propertyPath);
			return this;
		}

		/// <summary>
		/// Include reference properties
		/// </summary>
		/// <param name="propertyPaths"></param>
		public IQueryConstraints<TEntity> Include(List<string> propertyPaths)
		{
			Expandable.Include(propertyPaths);
			return this;
		}

		/// <summary>
		/// Sort ascending by a property
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		public IQueryConstraints<TEntity> SortBy(string propertyName)
		{
			Sortable.SortBy(propertyName);
			return this;
		}

		/// <summary>
		/// Sort descending by a property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		public IQueryConstraints<TEntity> SortByDescending(string propertyName)
		{
			Sortable.SortByDescending(propertyName);
			return this;
		}

		/// <summary>
		/// Property to sort by (ascending)
		/// </summary>
		/// <param name="property">The property.</param>
		public IQueryConstraints<TEntity> SortBy(Expression<Func<TEntity, object>> property)
		{
			Sortable.SortBy(property);
			return this;
		}

		/// <summary>
		/// Property to sort by (descending)
		/// </summary>
		/// <param name="property">The property</param>
		public IQueryConstraints<TEntity> SortByDescending(Expression<Func<TEntity, object>> property)
		{
			Sortable.SortByDescending(property);
			return this;
		}
	}
}
