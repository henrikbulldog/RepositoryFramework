using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RepositoryFramework.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;

namespace RepositoryFramework.EntityFramework
{
  /// <summary>
  /// Repository that uses Entity Framework Core 
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public class EntityFrameworkRepository<TEntity> :
    GenericRepositoryBase<TEntity>,
    IEntityFrameworkRepository<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityFrameworkRepository{TEntity}"/> class
    /// </summary>
    /// <param name="dbContext">Database context</param>
    /// <param name="autoCommit">Automatically save changes when data is modified</param>
    public EntityFrameworkRepository(
      DbContext dbContext,
      bool autoCommit = true)
    {
      if (dbContext == null)
      {
        throw new ArgumentNullException(nameof(dbContext));
      }

      DbContext = dbContext;
      AutoCommit = autoCommit;
    }

    /// <summary>
    /// Automatically save changes when data is modified
    /// </summary>
    public bool AutoCommit { get; private set; }

    /// <summary>
    /// Gets the database context
    /// </summary>
    protected virtual DbContext DbContext { get; private set; }

    /// <summary>
    /// Gets a list of reference properties to include
    /// </summary>
    public virtual List<string> Includes { get; private set; } = new List<string>();

    /// <summary>
    /// Gets number of items per page (when paging is used)
    /// </summary>
    public virtual int PageSize { get; private set; } = 0;

    /// <summary>
    /// Gets page number (one based index)
    /// </summary>
    public virtual int PageNumber { get; private set; } = 1;

    /// <summary>
    /// Gets the kind of sort order
    /// </summary>
    public virtual SortOrder SortOrder { get; private set; } = SortOrder.Unspecified;

    /// <summary>
    /// Gets property name for the property to sort by.
    /// </summary>
    public virtual string SortPropertyName { get; private set; } = null;

    /// <summary>
    /// Clear expansion
    /// </summary>
    /// <returns>Current instance</returns>
    public IRepository<TEntity> ClearIncludes()
    {
      Includes.Clear();
      return this;
    }

    /// <summary>
    /// Clear paging
    /// </summary>
    /// <returns>Current instance</returns>
    public IRepository<TEntity> ClearPaging()
    {
      PageSize = 0;
      PageNumber = 1;
      return this;
    }

    /// <summary>
    /// Clear sorting
    /// </summary>
    /// <returns>Current instance</returns>
    public IRepository<TEntity> ClearSorting()
    {
      SortPropertyName = null;
      SortOrder = SortOrder.Unspecified;
      return this;
    }

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
      if(AutoCommit)
      {
        SaveChanges();
      }
    }

    /// <summary>
    /// Create a new entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public async Task CreateAsync(TEntity entity)
    {
      if (entity == null)
      {
        throw new ArgumentNullException(nameof(entity));
      }

      DbContext.Set<TEntity>().Add(entity);
      if (AutoCommit)
      {
        await SaveChangesAsync();
      }
    }

    /// <summary>
    /// Create a list of new entities
    /// </summary>
    /// <param name="entities">List of entities</param>
    public virtual void CreateMany(IEnumerable<TEntity> entities)
    {
      if (entities == null)
      {
        throw new ArgumentNullException(nameof(entities));
      }

      DbContext.Set<TEntity>().AddRange(entities);
      if (AutoCommit)
      {
        SaveChanges();
      }
    }

    /// <summary>
    /// Create a list of new entities
    /// </summary>
    /// <param name="entities">List of entities</param>
    public async Task CreateManyAsync(IEnumerable<TEntity> entities)
    {
      if (entities == null)
      {
        throw new ArgumentNullException(nameof(entities));
      }

      DbContext.Set<TEntity>().AddRange(entities);
      if (AutoCommit)
      {
        await SaveChangesAsync();
      }
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
      if (AutoCommit)
      {
        SaveChanges();
      }
    }

    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public virtual async Task DeleteAsync(TEntity entity)
    {
      if (entity == null)
      {
        throw new ArgumentNullException(nameof(entity));
      }

      DbContext.Set<TEntity>().Remove(entity);
      if (AutoCommit)
      {
        await SaveChangesAsync();
      }
    }

    /// <summary>
    /// Delete a list of existing entities
    /// </summary>
    /// <param name="entities">Entity list</param>
    public virtual void DeleteMany(IEnumerable<TEntity> entities)
    {
      if (entities == null)
      {
        throw new ArgumentNullException(nameof(entities));
      }

      DbContext.Set<TEntity>().RemoveRange(entities);
      if (AutoCommit)
      {
        SaveChanges();
      }
    }

    /// <summary>
    /// Delete a list of existing entities
    /// </summary>
    /// <param name="entities">Entity list</param>
    public virtual async Task DeleteManyAsync(IEnumerable<TEntity> entities)
    {
      if (entities == null)
      {
        throw new ArgumentNullException(nameof(entities));
      }

      DbContext.Set<TEntity>().RemoveRange(entities);
      if (AutoCommit)
      {
        await SaveChangesAsync();
      }
    }

    /// <summary>
    /// Get a list of entities
    /// </summary>
    /// <returns>Query result</returns>
    public virtual IEnumerable<TEntity> Find()
    {
      return GetQuery().ToList();
    }

    /// <summary>
    /// Get a list of entities
    /// </summary>
    /// <returns>Query result</returns>
    public virtual async Task<IEnumerable<TEntity>> FindAsync()
    {
      return await GetQuery().ToListAsync();
    }

    /// <summary>
    /// Filters a collection of entities using a predicate
    /// </summary>
    /// <param name="where">Where predicate</param>
    /// <returns>Filtered collection of entities</returns>
    public virtual IEnumerable<TEntity> Find(Func<TEntity, bool> where)
    {
      return GetQuery()
        .Where(where)
        .ToList();
    }

    /// <summary>
    /// Filters a collection of entities using a predicate
    /// </summary>
    /// <param name="where">Where predicate</param>
    /// <returns>Filtered collection of entities</returns>
    public virtual async Task<IEnumerable<TEntity>> FindAsync(Func<TEntity, bool> where)
    {
      return await Task.Run(() => GetQuery()
        .Where(where)
        // ToListAsync() not supported after call to Where()
        .ToList());
    }

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="id">Filter to find a single item</param>
    /// <returns>Entity</returns>
    public virtual TEntity GetById(object id)
    {
      return GetByIdQuery(id).SingleOrDefault();
    }

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="id">Filter to find a single item</param>
    public async Task<TEntity> GetByIdAsync(object id)
    {
      return await GetByIdQuery(id).SingleOrDefaultAsync();
    }

    /// <summary>
    /// Include list of reference properties
    /// </summary>
    /// <param name="propertyPaths">Property paths</param>
    /// <returns>Current instance</returns>
    public IRepository<TEntity> Include(List<string> propertyPaths)
    {
      if (propertyPaths != null)
      {
        foreach (var propertyPath in propertyPaths)
        {
          Include(propertyPath);
        }
      }

      return this;
    }

    /// <summary>
    /// Include referenced properties
    /// </summary>
    /// <param name="propertyPaths">Comma-separated list of property paths</param>
    /// <returns>Current instance</returns>
    public IRepository<TEntity> Include(string propertyPaths)
    {
      if (!string.IsNullOrEmpty(propertyPaths))
      {
        var propertyPathList = propertyPaths
          .Split(',')
          .Select(r => r.Trim())
          .Where(s => !string.IsNullOrWhiteSpace(s));
        foreach (var propertyPath in propertyPathList)
        {
          var validatedPropertyPath = propertyPath;
          if (!TryCheckPropertyPath(propertyPath, out validatedPropertyPath))
          {
            throw new ArgumentException(
              string.Format(
                "'{0}' is not a valid property path of '{1}'.",
                propertyPath,
                EntityTypeName));
          }
          if (!Includes.Contains(validatedPropertyPath))
          {
            Includes.Add(validatedPropertyPath);
          }
        }
      }

      return this;
    }

    /// <summary>
    /// Include reference property
    /// </summary>
    /// <param name="property">Property expression</param>
    /// <returns>Current instance</returns>
    public IRepository<TEntity> Include(Expression<Func<TEntity, object>> property)
    {
      var propertyPath = GetPropertyName(property);
      if (!TryCheckPropertyPath(propertyPath, out propertyPath))
      {
        throw new ArgumentException(
            string.Format(
              "'{0}' is not a valid property path of '{1}'.",
              propertyPath,
              EntityTypeName));
      }

      Include(propertyPath);
      return this;
    }

    /// <summary>
    /// Use paging
    /// </summary>
    /// <param name="pageNumber">Page to get (one based index).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>Current instance</returns>
    public IRepository<TEntity> Page(int pageNumber, int pageSize)
    {
      PageSize = pageSize;
      PageNumber = pageNumber;
      return this;
    }

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>Current instance</returns>
    public IRepository<TEntity> SortBy(Expression<Func<TEntity, object>> property)
    {
      if (property == null)
      {
        throw new ArgumentNullException(nameof(property));
      }

      var name = GetPropertyName(property);
      SortBy(name);
      return this;
    }

    /// <summary>
    /// Sort ascending by a property
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    public IRepository<TEntity> SortBy(string propertyName)
    {
      if (propertyName == null)
      {
        throw new ArgumentNullException(nameof(propertyName));
      }

      ValidatePropertyName(propertyName, out propertyName);

      SortOrder = SortOrder.Ascending;
      SortPropertyName = propertyName;
      return this;
    }

    /// <summary>
    /// Property to sort by (descending)
    /// </summary>
    /// <param name="property">The property</param>
    /// <returns>Current instance</returns>
    public IRepository<TEntity> SortByDescending(Expression<Func<TEntity, object>> property)
    {
      if (property == null)
      {
        throw new ArgumentNullException(nameof(property));
      }

      var name = GetPropertyName(property);
      SortByDescending(name);
      return this;
    }

    /// <summary>
    /// Sort descending by a property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    public IRepository<TEntity> SortByDescending(string propertyName)
    {
      if (propertyName == null)
      {
        throw new ArgumentNullException(nameof(propertyName));
      }

      ValidatePropertyName(propertyName, out propertyName);

      SortOrder = SortOrder.Descending;
      SortPropertyName = propertyName;
      return this;
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
      if (AutoCommit)
      {
        SaveChanges();
      }
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public async Task UpdateAsync(TEntity entity)
    {
      if (entity == null)
      {
        throw new ArgumentNullException(nameof(entity));
      }

      DbContext.Set<TEntity>().Update(entity);
      if (AutoCommit)
      {
        await SaveChangesAsync();
      }
    }

    /// <summary>
    /// Persists all changes to the data storage
    /// <returns>Current instance</returns>
    /// </summary>
    public IRepository<TEntity> SaveChanges()
    {
      DbContext.SaveChanges();
      return this;
    }

    /// <summary>
    /// Persists all changes to the data storage
    /// <returns>Current instance</returns>
    /// </summary>
    public async Task<IRepository<TEntity>> SaveChangesAsync()
    {
      await DbContext.SaveChangesAsync();
      return this;
    }

    /// <summary>
    /// Detaches all entites from the repository
    /// <returns>Current instance</returns>
    /// </summary>
    public IRepository<TEntity> DetachAll()
    {
      foreach (var entityEntry in DbContext.ChangeTracker.Entries().ToArray())
      {
        if (entityEntry.Entity != null)
        {
          entityEntry.State = EntityState.Detached;
        }
      }
      return this;
    }

    /// <summary>
    /// Detaches all entites from the repository
    /// <returns>Current instance</returns>
    /// </summary>
    public async Task<IRepository<TEntity>> DetachAllAsync()
    {
      Task task = Task.Run(() => DetachAll());
      await task;
      return this;
    }

    /// <summary>
    /// Gets a queryable collection of entities
    /// </summary>
    /// <returns>Queryable collection of entities</returns>
    public IQueryable<TEntity> AsQueryable()
    {
      return GetQuery();
    }

    /// <summary>
    /// Gets a collection of entities with sorting, paging and include constraints
    /// </summary>
    /// <returns></returns>
    protected virtual IQueryable<TEntity> GetQuery()
    {
      IQueryable<TEntity> query = DbContext.Set<TEntity>();

      foreach (var propertyName in Includes)
      {
        query = query.Include(propertyName);
      }

      return query
        .Sort(this)
        .Page(this);
    }

    /// <summary>
    /// Gets a query to find a single entity by id
    /// </summary>
    /// <param name="id">Entoty id</param>
    /// <returns>Query</returns>
    protected virtual IQueryable<TEntity> GetByIdQuery(object id)
    {
      var p = Expression.Parameter(EntityType);
      var prop = Expression.Property(p, IdPropertyName);
      var body = Expression.Equal(prop, Expression.Constant(id));
      var exp = Expression.Lambda(body, p);
      var idCompare = (Func<TEntity, bool>)exp.Compile();

      IQueryable<TEntity> query = DbContext.Set<TEntity>();
      foreach (var propertyName in Includes)
      {
        query = query.Include(propertyName);
      }
      return query;
    }
  }
}
