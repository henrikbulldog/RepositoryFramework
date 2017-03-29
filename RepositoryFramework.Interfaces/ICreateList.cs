using System.Collections.Generic;

namespace RepositoryFramework.Interfaces
{
	/// <summary>
	/// Creates a list of new entities.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface ICreateList<TEntity> where TEntity : class
	{
    /// <summary>
    /// Create a list of new entities
    /// </summary>
    /// <param name="entities">List of entities</param>
    void Create(IEnumerable<TEntity> entities);
	}
}