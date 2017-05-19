using RepositoryFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RepositoryFramework.MongoDB
{
  /// <summary>
  /// IRepository extension methods
  /// </summary>
  public static class IRepositoryExtensions
  {
    /// <summary>
    /// Use paging
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="pageNumber">Page to get (one based index).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> Page<TEntity>(
      this IRepository<TEntity> instance,
      int pageNumber,
      int pageSize)
      where TEntity : class
    {
      var mongoDBRepository = instance as IMongoDBRepository<TEntity>;
      if (mongoDBRepository == null)
      {
        throw new NotImplementedException();
      }

      mongoDBRepository.Page(pageNumber, pageSize);
      return instance;
    }

    /// <summary>
    /// Clear paging
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> ClearPaging<TEntity>(
      this IRepository<TEntity> instance)
      where TEntity : class
    {
      var mongoDBRepository = instance as IMongoDBRepository<TEntity>;
      if (mongoDBRepository == null)
      {
        throw new NotImplementedException();
      }

      mongoDBRepository.ClearPaging();
      return instance;
    }

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="property">The property.</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> SortBy<TEntity>(
      this IRepository<TEntity> instance,
      Expression<Func<TEntity, object>> property)
      where TEntity : class
    {
      var mongoDBRepository = instance as IMongoDBRepository<TEntity>;
      if (mongoDBRepository == null)
      {
        throw new NotImplementedException();
      }

      mongoDBRepository.SortBy(property);
      return instance;
    }

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> SortBy<TEntity>(
      this IRepository<TEntity> instance,
      string propertyName)
      where TEntity : class
    {
      var mongoDBRepository = instance as IMongoDBRepository<TEntity>;
      if (mongoDBRepository == null)
      {
        throw new NotImplementedException();
      }

      mongoDBRepository.SortBy(propertyName);
      return instance;
    }

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="property">The property.</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> SortByDescending<TEntity>(
      this IRepository<TEntity> instance,
      Expression<Func<TEntity, object>> property)
      where TEntity : class
    {
      var mongoDBRepository = instance as IMongoDBRepository<TEntity>;
      if (mongoDBRepository == null)
      {
        throw new NotImplementedException();
      }

      mongoDBRepository.SortByDescending(property);
      return instance;
    }

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> SortByDescending<TEntity>(
      this IRepository<TEntity> instance,
      string propertyName)
      where TEntity : class
    {
      var mongoDBRepository = instance as IMongoDBRepository<TEntity>;
      if (mongoDBRepository == null)
      {
        throw new NotImplementedException();
      }

      mongoDBRepository.SortByDescending(propertyName);
      return instance;
    }

    /// <summary>
    /// Filters a sequence of entities using a predicate
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="predicate">Predicate</param>
    /// <returns>Current instance</returns>
    public static IEnumerable<TEntity> Find<TEntity>(
      this IRepository<TEntity> instance,
      Func<TEntity, bool> predicate)
      where TEntity : class
    {
      var mongoDBRepository = instance as IMongoDBRepository<TEntity>;
      if (mongoDBRepository == null)
      {
        throw new NotImplementedException();
      }

      return mongoDBRepository.Find(predicate);
    }

    /// <summary>
    /// Filters a sequence of entities using a predicate
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="predicate">Predicate</param>
    /// <returns>Current instance</returns>
    public static async Task<IEnumerable<TEntity>> FindAsync<TEntity>(
      this IRepository<TEntity> instance,
      Func<TEntity, bool> predicate)
      where TEntity : class
    {
      var mongoDBRepository = instance as IMongoDBRepository<TEntity>;
      if (mongoDBRepository == null)
      {
        throw new NotImplementedException();
      }

      return await mongoDBRepository.FindAsync(predicate);
    }

    /// <summary>
    /// Include list of reference properties
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="propertyPaths">Property paths</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> Include<TEntity>(
      this IRepository<TEntity> instance,
      List<string> propertyPaths)
      where TEntity : class
    {
      var mongoDBRepository = instance as IMongoDBRepository<TEntity>;
      if (mongoDBRepository == null)
      {
        throw new NotImplementedException();
      }

      mongoDBRepository.Include(propertyPaths);
      return instance;
    }

    /// <summary>
    /// Clear sorting
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> ClearSorting<TEntity>(
      this IRepository<TEntity> instance)
      where TEntity : class
    {
      var mongoDBRepository = instance as IMongoDBRepository<TEntity>;
      if (mongoDBRepository == null)
      {
        throw new NotImplementedException();
      }

      mongoDBRepository.ClearSorting();
      return instance;
    }

    /// <summary>
    /// Clear includes
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <returns>Current instance</returns>
    public static IQueryable<TEntity> AsQueryable<TEntity>(
      this IRepository<TEntity> instance)
      where TEntity : class
    {
      var mongoDBRepository = instance as IMongoDBRepository<TEntity>;
      if (mongoDBRepository == null)
      {
        throw new NotImplementedException();
      }

      return mongoDBRepository.AsQueryable();
    }

    /// <summary>
    /// Filters a collection of entities
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="filter">Filter</param>
    /// <returns>Filtered collection of entities</returns>
    public static IEnumerable<TEntity> Find<TEntity>(
      this IRepository<TEntity> instance,
      string filter)
      where TEntity : class
    {
      var mongoDBRepository = instance as IMongoDBRepository<TEntity>;
      if (mongoDBRepository == null)
      {
        throw new NotImplementedException();
      }

      return mongoDBRepository.Find(filter);
    }

    /// <summary>
    /// Filters a collection of entities
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="filter">Filter</param>
    /// <returns>Filtered collection of entities</returns>
    public static async Task<IEnumerable<TEntity>> FindAsync<TEntity>(
      this IRepository<TEntity> instance,
      string filter)
      where TEntity : class
    {
      var mongoDBRepository = instance as IMongoDBRepository<TEntity>;
      if (mongoDBRepository == null)
      {
        throw new NotImplementedException();
      }

      return await mongoDBRepository.FindAsync(filter);
    }
  }
}
