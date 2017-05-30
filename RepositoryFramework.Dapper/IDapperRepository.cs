﻿using RepositoryFramework.Interfaces;
using System;
using System.Linq.Expressions;

namespace RepositoryFramework.Dapper
{
  /// <summary>
  /// Repository that uses the Dapper micro-ORM framework, see https://github.com/StackExchange/Dapper
  /// </summary>
  /// <typeparam name="TEntity"></typeparam>
  public interface IDapperRepository<TEntity> :
    IRepository<TEntity>,
    ISortableRepository<TEntity>,
    IPageableRepository<TEntity>,
    IFindSql<TEntity>,
    IFindSqlAsync<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Use paging
    /// </summary>
    /// <param name="pageNumber">Page to get (one based index).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>Current instance</returns>
    new IDapperRepository<TEntity> Page(int pageNumber, int pageSize);

    /// <summary>
    /// Clear paging
    /// </summary>
    /// <returns>Current instance</returns>
    new IDapperRepository<TEntity> ClearPaging();

    /// <summary>
    /// Sort ascending by a property
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    new IDapperRepository<TEntity> SortBy(string propertyName);

    /// <summary>
    /// Sort descending by a property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    new IDapperRepository<TEntity> SortByDescending(string propertyName);

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>Current instance</returns>
    new IDapperRepository<TEntity> SortBy(Expression<Func<TEntity, object>> property);

    /// <summary>
    /// Property to sort by (descending)
    /// </summary>
    /// <param name="property">The property</param>
    /// <returns>Current instance</returns>
    new IDapperRepository<TEntity> SortByDescending(Expression<Func<TEntity, object>> property);

    /// <summary>
    /// Clear sorting
    /// </summary>
    /// <returns>Current instance</returns>
    new IDapperRepository<TEntity> ClearSorting();
  }
}
