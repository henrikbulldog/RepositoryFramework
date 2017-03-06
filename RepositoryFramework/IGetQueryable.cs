using System;
using System.Linq;

namespace RepositoryFramework
{
	/// <summary>
	/// Gets an entity by a lambda expression.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IGetQueryable<TEntity> where TEntity : class
	{
		/// <summary>
		/// Gets an entity by a lambda expression.
		/// </summary>
		/// <param name="filter">Lambda</param>
		TEntity GetById(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter);
		/// <summary>
		/// Gets an entity by a lambda expression and query constraints for exapanding (eager loading) navigatinal properties and collections.
		/// </summary>
		/// <param name="filter">Lambda</param>
		/// <param name="constraints">Query constraints for exapanding (eager loading) navigatinal properties and collections</param>
		/// <returns></returns>
		TEntity GetById(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter, IExpandable<TEntity> constraints);
	}
}
