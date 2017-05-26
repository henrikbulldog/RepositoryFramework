using RepositoryFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RepositoryFramework.MongoDB
{
  /// <summary>
  /// Repository that uses the MongoDB document database, see https://docs.mongodb.com/
  /// </summary>
  /// <typeparam name="TEntity"></typeparam>
  public interface IMongoDBRepository<TEntity> :
    IRepository<TEntity>,
    ISortableRepository<TEntity>,
    IPageableRepository<TEntity>,
    IQueryableRepository<TEntity>,
    IFindWhere<TEntity>,
    IFindWhereAsync<TEntity>,
    IFindFilter<TEntity>,
    IFindFilterAsync<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Use paging
    /// </summary>
    /// <param name="pageNumber">Page to get (one based index).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>Current instance</returns>
    new IMongoDBRepository<TEntity> Page(int pageNumber, int pageSize);

    /// <summary>
    /// Clear paging
    /// </summary>
    /// <returns>Current instance</returns>
    new IMongoDBRepository<TEntity> ClearPaging();

    /// <summary>
    /// Sort ascending by a property
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    new IMongoDBRepository<TEntity> SortBy(string propertyName);

    /// <summary>
    /// Sort descending by a property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    new IMongoDBRepository<TEntity> SortByDescending(string propertyName);

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>Current instance</returns>
    new IMongoDBRepository<TEntity> SortBy(Expression<Func<TEntity, object>> property);

    /// <summary>
    /// Property to sort by (descending)
    /// </summary>
    /// <param name="property">The property</param>
    /// <returns>Current instance</returns>
    new IMongoDBRepository<TEntity> SortByDescending(Expression<Func<TEntity, object>> property);

    /// <summary>
    /// Clear sorting
    /// </summary>
    /// <returns>Current instance</returns>
    new IMongoDBRepository<TEntity> ClearSorting();
  }
}
