using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RepositoryFramework
{
	/// <summary>
	/// Can expand (or eager load) navigational properties and collections of an entity.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IExpandable<TEntity> where TEntity : class
	{
		/// <summary>
		/// List of reference properties to include
		/// </summary>
		List<string> Includes { get; }

    /// <summary>
    /// Include reference property
    /// </summary>
    /// <param name="propertyPath"></param>
    IExpandable<TEntity> Include(string propertyPath);

    /// <summary>
    /// Include reference property
    /// </summary>
    /// <param name="property">Property expression</param>
    IExpandable<TEntity> Include(Expression<Func<TEntity, object>> property);

    /// <summary>
    /// Include reference properties
    /// </summary>
    /// <param name="propertyPaths"></param>
    IExpandable<TEntity> Include(List<string> propertyPaths);
	}
}