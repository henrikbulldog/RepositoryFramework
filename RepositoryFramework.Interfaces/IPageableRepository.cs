namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Pages a result sets
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IPageableRepository<TEntity> : IRepository<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Gets number of items per page (when paging is used)
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Gets page number (one based index)
    /// </summary>
    int PageNumber { get; }

    /// <summary>
    /// Gets the total number of items available in this set. For example, if a user has 100 blog posts, the response may only contain 10 items, but the totalItems would be 100.
    /// </summary>
    long TotalItems { get; }

    /// <summary>
    /// Gets the index of the first item. For consistency, startIndex should be 1-based. For example, the first item in the first set of items should have a startIndex of 1. If the user requests the next set of data, the startIndex may be 10.
    /// </summary>
    int StartIndex { get; }

    /// <summary>
    /// Gets the total number of pages in the result set.
    /// </summary>
    int TotalPages { get; }

    /// <summary>
    /// Use paging
    /// </summary>
    /// <param name="pageNumber">Page to get (one based index).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>Current instance</returns>
    IPageableRepository<TEntity> Page(int pageNumber, int pageSize);

    /// <summary>
    /// Clear paging
    /// </summary>
    /// <returns>Current instance</returns>
    IPageableRepository<TEntity> ClearPaging();
  }
}