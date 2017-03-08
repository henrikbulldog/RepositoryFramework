namespace RepositoryFramework.Interfaces
{
	/// <summary>
	/// Finds a list of entites using an object with filtering information.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TFilter"></typeparam>
	public interface IFindFilter<TEntity, TFilter> where TEntity : class
	{
		IQueryResult<TEntity> Find(TFilter filter);
	}
}
