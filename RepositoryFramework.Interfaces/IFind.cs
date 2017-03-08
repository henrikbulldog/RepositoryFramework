namespace RepositoryFramework.Interfaces
{
	/// <summary>
	/// Finds a list of entites.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IFind<TEntity> where TEntity : class
  {
		IQueryResult<TEntity> Find();
	}
}
