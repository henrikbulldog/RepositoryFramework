namespace RepositoryFramework
{
	/// <summary>
	/// Updates an etity.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IUpdate<TEntity> where TEntity : class
  {
    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    void Update(TEntity entity);
  }
}
