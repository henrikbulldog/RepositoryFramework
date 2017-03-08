namespace RepositoryFramework.Interfaces
{
	/// <summary>
	/// Gets an entity by id.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TFilter"></typeparam>
	public interface IGet<TEntity, in TFilter> where TEntity : class
  {
    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="id">Id</param>
    TEntity GetById(TFilter filter);
  }
}
