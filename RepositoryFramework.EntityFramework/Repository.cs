using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.EntityFramework
{
  /// <summary>
  /// Creates, updates and deletes entities.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public class Repository<TEntity> :
    IRepository<TEntity>,
    IFindQueryable<TEntity>,
    IGetQueryable<TEntity>,
    IUnitOfWork
    where TEntity : class
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class
    /// </summary>
    /// <param name="dbContext">Database context</param>
    public Repository(DbContext dbContext)
    {
      if (dbContext == null)
      {
        throw new ArgumentNullException(nameof(dbContext));
      }

      DbContext = dbContext;
    }

    /// <summary>
    /// Gets the database context
    /// </summary>
    protected DbContext DbContext { get; private set; }

    /// <summary>
    /// Create a new entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public virtual void Create(TEntity entity)
    {
      if (entity == null)
      {
        throw new ArgumentNullException(nameof(entity));
      }

      DbContext.Set<TEntity>().Add(entity);
    }

    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public virtual void Delete(TEntity entity)
    {
      if (entity == null)
      {
        throw new ArgumentNullException(nameof(entity));
      }

      DbContext.Set<TEntity>().Remove(entity);
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public virtual void Update(TEntity entity)
    {
      if (entity == null)
      {
        throw new ArgumentNullException(nameof(entity));
      }

      DbContext.Set<TEntity>().Update(entity);
    }

    /// <summary>
    /// Finds a list of entites using a filter expression.
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <param name="constraints">Query constraints for expansion, paging and sorting</param>
    /// <returns>Current instance</returns>
    public virtual IQueryResult<TEntity> Find(
      Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
      IQueryConstraints<TEntity> constraints)
    {
      var query = Filter(filter);

      IEnumerable<TEntity> items;

      if (constraints != null)
      {
        items = constraints.ApplyTo(query).ToList();
      }
      else
      {
        items = query.ToList();
      }

      var count = query.Count();

      return new QueryResult<TEntity>(items, count);
    }

    /// <summary>
    /// Finds a list of entites using a filter expression.
    /// </summary>
    /// <returns>Current instance</returns>
    public virtual IQueryResult<TEntity> Find()
    {
      return Find(null, null);
    }

    /// <summary>
    /// Finds a list of entites using a filter expression.
    /// </summary>
    /// <param name="constraints">Query constraints for expansion, paging and sorting</param>
    /// <returns>Current instance</returns>
    public virtual IQueryResult<TEntity> Find(IQueryConstraints<TEntity> constraints)
    {
      return Find(null, constraints);
    }

    /// <summary>
    /// Finds a list of entites using a filter expression.
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <returns>Current instance</returns>
    public virtual IQueryResult<TEntity> Find(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
    {
      return Find(filter, null);
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public virtual void Dispose()
    {
    }

    /// <summary>
    /// Commit changes
    /// </summary>
    public virtual void SaveChanges()
    {
      DbContext.SaveChanges();
    }

    /// <summary>
    /// Detach all objects from the database context; Disable all expansions
    /// </summary>
    public virtual void DetachAll()
    {
      foreach (var entityEntry in DbContext.ChangeTracker.Entries().ToArray())
      {
        if (entityEntry.Entity != null)
        {
          entityEntry.State = EntityState.Detached;
        }
      }
    }

    /// <summary>
    /// Gets an entity by a filter expression.
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <returns>Entity</returns>
    public virtual TEntity GetById(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
    {
      return GetById(filter, null);
    }

    /// <summary>
    /// Gets an entity by a filter expression and query constraints for exapanding (eager loading) navigatinal properties and collections.
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <param name="includes">Query constraints for exapanding (eager loading) navigatinal properties and collections</param>
    /// <returns>Entity</returns>
    public virtual TEntity GetById(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter, IExpandable<TEntity> includes)
    {
      var query = Filter(filter);
      if (includes != null)
      {
        query = includes.AddExpansion(query);
      }

      return query.SingleOrDefault();
    }

    private IQueryable<TEntity> Filter(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
    {
      IQueryable<TEntity> query;

      if (filter != null)
      {
        query = filter(DbContext.Set<TEntity>().AsQueryable());
      }
      else
      {
        query = DbContext.Set<TEntity>();
      }

      return query;
    }
  }
}
