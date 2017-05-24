using RepositoryFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RepositoryFramework.EntityFramework
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
      var entityFrameworkRepository = instance as IEntityFrameworkRepository<TEntity>;
      if (entityFrameworkRepository == null)
      {
        throw new NotImplementedException();
      }

      entityFrameworkRepository.Page(pageNumber, pageSize);
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
      var entityFrameworkRepository = instance as IEntityFrameworkRepository<TEntity>;
      if (entityFrameworkRepository == null)
      {
        throw new NotImplementedException();
      }

      entityFrameworkRepository.ClearPaging();
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
      var entityFrameworkRepository = instance as IEntityFrameworkRepository<TEntity>;
      if (entityFrameworkRepository == null)
      {
        throw new NotImplementedException();
      }

      entityFrameworkRepository.SortBy(property);
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
      var entityFrameworkRepository = instance as IEntityFrameworkRepository<TEntity>;
      if (entityFrameworkRepository == null)
      {
        throw new NotImplementedException();
      }

      entityFrameworkRepository.SortBy(propertyName);
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
      var entityFrameworkRepository = instance as IEntityFrameworkRepository<TEntity>;
      if (entityFrameworkRepository == null)
      {
        throw new NotImplementedException();
      }

      entityFrameworkRepository.SortByDescending(property);
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
      var entityFrameworkRepository = instance as IEntityFrameworkRepository<TEntity>;
      if (entityFrameworkRepository == null)
      {
        throw new NotImplementedException();
      }

      entityFrameworkRepository.SortByDescending(propertyName);
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
      var entityFrameworkRepository = instance as IEntityFrameworkRepository<TEntity>;
      if (entityFrameworkRepository == null)
      {
        throw new NotImplementedException();
      }

      entityFrameworkRepository.ClearSorting();
      return instance;
    }

    /// <summary>
    /// Include reference property
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="propertyPath">Property path</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> Include<TEntity>(
      this IRepository<TEntity> instance,
      string propertyPath)
      where TEntity : class
    {
      var entityFrameworkRepository = instance as IEntityFrameworkRepository<TEntity>;
      if (entityFrameworkRepository == null)
      {
        throw new NotImplementedException();
      }

      entityFrameworkRepository.Include(propertyPath);
      return instance;
    }

    /// <summary>
    /// Include referenced properties
    /// </summary>
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <param name="propertyPaths">Comma-separated list of property paths</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> Include<TEntity>(
      this IRepository<TEntity> instance,
      Expression<Func<TEntity, object>> propertyPaths)
      where TEntity : class
    {
      var entityFrameworkRepository = instance as IEntityFrameworkRepository<TEntity>;
      if (entityFrameworkRepository == null)
      {
        throw new NotImplementedException();
      }

      entityFrameworkRepository.Include(propertyPaths);
      return instance;
    }

    /// <summary>
    /// Clear includes
    /// </summary>
    /// <param name="instance">Current instance</param>
    /// <returns>Current instance</returns>
    public static IRepository<TEntity> ClearIncludes<TEntity>(
      this IRepository<TEntity> instance)
      where TEntity : class
    {
      var entityFrameworkRepository = instance as IEntityFrameworkRepository<TEntity>;
      if (entityFrameworkRepository == null)
      {
        throw new NotImplementedException();
      }

      entityFrameworkRepository.ClearIncludes();
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
      var entityFrameworkRepository = instance as IEntityFrameworkRepository<TEntity>;
      if (entityFrameworkRepository == null)
      {
        throw new NotImplementedException();
      }

      return entityFrameworkRepository.AsQueryable();
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
      var entityFrameworkRepository = instance as IEntityFrameworkRepository<TEntity>;
      if (entityFrameworkRepository == null)
      {
        throw new NotImplementedException();
      }

      return entityFrameworkRepository.Find(predicate);
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
      var entityFrameworkRepository = instance as IEntityFrameworkRepository<TEntity>;
      if (entityFrameworkRepository == null)
      {
        throw new NotImplementedException();
      }

      return await entityFrameworkRepository.FindAsync(predicate);
    }
  }
}
