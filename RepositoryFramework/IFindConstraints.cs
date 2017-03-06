namespace RepositoryFramework
{
	/// <summary>
	/// Finds a list of entities using paging and sorting constraints.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IFindConstraints<TEntity> : IFind<TEntity> where TEntity : class
  {
		IQueryResult<TEntity> Find(IQueryConstraints<TEntity> constraints);
	}
}
