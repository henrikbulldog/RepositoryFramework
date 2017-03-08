using System;
using System.Linq;

namespace RepositoryFramework.Interfaces
{
	/// <summary>
	/// Finds a list of entites using a lambda expression for filtering.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IFindQueryable<TEntity> : IFindConstraints<TEntity> where TEntity : class
	{
		/// <summary>
		/// Finds a list of entites using a lambda expression for filtering.
		/// </summary>
		/// <param name="filter">Lambda</param>
		IQueryResult<TEntity> Find(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter);
		/// <summary>
		/// Finds a list of entites using a lambda expression for filtering and query constraints for expansion, paging and sorting.
		/// </summary>
		/// <param name="filter">Lambda</param>
		/// <param name="constraints">Query constraints for expansion, paging and sorting</param>
		IQueryResult<TEntity> Find(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter, IQueryConstraints<TEntity> constraints);
	}
}