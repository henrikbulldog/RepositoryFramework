using System;

namespace RepositoryFramework
{
	/// <summary>
	/// Saves changes made to a repository context and detaches all entities from the context.
	/// </summary>
	public interface IUnitOfWork : IDisposable
	{
		/// <summary>
		/// Persist all changes to the data storage
		/// </summary>
		void SaveChanges();
		/// <summary>
		/// Detach all entites from the repository
		/// </summary>
		void DetachAll();
	}
}
