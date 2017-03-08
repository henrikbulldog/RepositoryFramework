namespace RepositoryFramework.Interfaces
{
	/// <summary>
	/// Creates a new entity.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface ICreate<TEntity> where TEntity : class
	{
		/// <summary>
		/// Create a new entity
		/// </summary>
		/// <param name="entity">Entity</param>
		void Create(TEntity entity);
	}
}