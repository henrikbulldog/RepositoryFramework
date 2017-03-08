namespace RepositoryFramework.Interfaces
{
	/// <summary>
	/// Creates, updates and deletes entities.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IRepository<TEntity>
		: ICreate<TEntity>,
		IUpdate<TEntity>,
		IDelete<TEntity>
		where TEntity : class
	{
	}
}