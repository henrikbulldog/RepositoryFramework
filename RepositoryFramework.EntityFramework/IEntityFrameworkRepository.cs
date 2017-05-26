using RepositoryFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RepositoryFramework.EntityFramework
{
  /// <summary>
  /// Repository that uses Entity Framework Core 
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public interface IEntityFrameworkRepository<TEntity> :
    IRepository<TEntity>,
    IExpandableRepository<TEntity>,
    ISortableRepository<TEntity>,
    IPageableRepository<TEntity>,
    IQueryableRepository<TEntity>,
    IUnitOfWorkRepository<TEntity>,
    IUnitOfWorkRepositoryAsync<TEntity>,
    IFindWhere<TEntity>,
    IFindWhereAsync<TEntity>
  where TEntity : class
  {
    /// <summary>
    /// Use paging
    /// </summary>
    /// <param name="pageNumber">Page to get (one based index).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>Current instance</returns>
    new IEntityFrameworkRepository<TEntity> Page(int pageNumber, int pageSize);

    /// <summary>
    /// Clear paging
    /// </summary>
    /// <returns>Current instance</returns>
    new IEntityFrameworkRepository<TEntity> ClearPaging();

    /// <summary>
    /// Include referenced properties
    /// </summary>
    /// <param name="propertyPaths">Comma-separated list of property paths</param>
    /// <returns>Current instance</returns>
    new IEntityFrameworkRepository<TEntity> Include(string propertyPaths);

    /// <summary>
    /// Include referenced property
    /// </summary>
    /// <param name="property">Property expression</param>
    /// <returns>Current instance</returns>
    new IEntityFrameworkRepository<TEntity> Include(Expression<Func<TEntity, object>> property);

    /// <summary>
    /// Clear includes
    /// </summary>
    /// <returns>Current instance</returns>
    new IEntityFrameworkRepository<TEntity> ClearIncludes();

    /// <summary>
    /// Sort ascending by a property
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    new IEntityFrameworkRepository<TEntity> SortBy(string propertyName);

    /// <summary>
    /// Sort descending by a property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    new IEntityFrameworkRepository<TEntity> SortByDescending(string propertyName);

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>Current instance</returns>
    new IEntityFrameworkRepository<TEntity> SortBy(Expression<Func<TEntity, object>> property);

    /// <summary>
    /// Property to sort by (descending)
    /// </summary>
    /// <param name="property">The property</param>
    /// <returns>Current instance</returns>
    new IEntityFrameworkRepository<TEntity> SortByDescending(Expression<Func<TEntity, object>> property);

    /// <summary>
    /// Clear sorting
    /// </summary>
    /// <returns>Current instance</returns>
    new IEntityFrameworkRepository<TEntity> ClearSorting();
  }
}
