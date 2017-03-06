namespace RepositoryFramework
{
	/// <summary>
	/// Deletes an entity.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IDelete<TEntity> where TEntity : class
  {
    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    void Delete(TEntity entity);
  }
}
