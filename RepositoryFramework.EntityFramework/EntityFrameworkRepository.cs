namespace RepositoryFramework.EntityFramework
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using RepositoryFramework.Interfaces;

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
    /// <param name="calculateTotalItems">Set whether TotalItems is calculated on Find()</param>
    /// <param name="idProperty">Id property expression</param>
    public EntityFrameworkRepository(
      DbContext dbContext,
      Expression<Func<TEntity, object>> idProperty = null,
      bool autoCommit = true,
      bool calculateTotalItems = true)
      : base(idProperty)
    {
      if (dbContext == null)
      {
        throw new ArgumentNullException(nameof(dbContext));
      }

      DbContext = dbContext;
      AutoCommit = autoCommit;
      CalculateTotalItems = calculateTotalItems;
    }

    /// <summary>
    /// Gets or sets a value indicating whether to automatically save changes when data is modified
    /// </summary>
    public bool AutoCommit { get; set; }

    /// <summary>
    /// Gets a value indicating whether TotalItems is calculated on Find()
    /// </summary>
    public bool CalculateTotalItems { get; }

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
    /// Gets the total number of items available in this set. For example, if a user has 100 blog posts, the response may only contain 10 items, but the totalItems would be 100.
    /// </summary>
    public virtual long TotalItems { get; private set; } = 0;

    /// <summary>
    /// Gets the index of the first item. For consistency, startIndex should be 1-based. For example, the first item in the first set of items should have a startIndex of 1. If the user requests the next set of data, the startIndex may be 10.
    /// </summary>
    public virtual int StartIndex
    {
      get { return PageNumber < 2 ? 1 : ((PageNumber - 1) * PageSize) + 1; }
    }

    /// <summary>
    /// Gets the total number of pages in the result set.
    /// </summary>
    public virtual int TotalPages
    {
      get { return PageSize == 0 ? 1 : (int)(TotalItems / PageSize) + 1; }
    }

    /// <summary>
    /// Gets the kind of sort order
    /// </summary>
    public virtual Interfaces.SortOrder SortOrder { get; private set; } = Interfaces.SortOrder.Unspecified;

    /// <summary>
    /// Gets property name for the property to sort by.
    /// </summary>
    public virtual string SortPropertyName { get; private set; } = null;

    /// <summary>
    /// Gets the database context
    /// </summary>
    protected virtual DbContext DbContext { get; private set; }

    /// <summary>
    /// Clear expansion
    /// </summary>
    /// <returns>Current instance</returns>
    IExpandableRepository<TEntity> IExpandableRepository<TEntity>.ClearIncludes()
    {
      return ClearIncludes();
    }

    /// <summary>
    /// Clear expansion
    /// </summary>
    /// <returns>Current instance</returns>
    public IEntityFrameworkRepository<TEntity> ClearIncludes()
    {
      Includes.Clear();
      return this;
    }

    /// <summary>
    /// Clear paging
    /// </summary>
    /// <returns>Current instance</returns>
    IPageableRepository<TEntity> IPageableRepository<TEntity>.ClearPaging()
    {
      return ClearPaging();
    }

    /// <summary>
    /// Clear paging
    /// </summary>
    /// <returns>Current instance</returns>
    public IEntityFrameworkRepository<TEntity> ClearPaging()
    {
      PageSize = 0;
      PageNumber = 1;
      return this;
    }

    /// <summary>
    /// Clear sorting
    /// </summary>
    /// <returns>Current instance</returns>
    ISortableRepository<TEntity> ISortableRepository<TEntity>.ClearSorting()
    {
      return ClearSorting();
    }

    /// <summary>
    /// Clear sorting
    /// </summary>
    /// <returns>Current instance</returns>
    public IEntityFrameworkRepository<TEntity> ClearSorting()
    {
      SortPropertyName = null;
      SortOrder = Interfaces.SortOrder.Unspecified;
      return this;
    }

    /// <summary>
    /// Create a new entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public virtual void Create(TEntity entity)
    {
      var task = CreateAsync(entity);
      task.WaitSync();
    }

    /// <summary>
    /// Create a new entity
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Task</returns>
    public virtual async Task CreateAsync(TEntity entity)
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
      var task = CreateManyAsync(entities);
      task.WaitSync();
    }

    /// <summary>
    /// Create a list of new entities
    /// </summary>
    /// <param name="entities">List of entities</param>
    /// <returns>Task</returns>
    public virtual async Task CreateManyAsync(IEnumerable<TEntity> entities)
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
      var task = DeleteAsync(entity);
      task.WaitSync();
    }

    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Task</returns>
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
      var task = DeleteManyAsync(entities);
      task.WaitSync();
    }

    /// <summary>
    /// Delete a list of existing entities
    /// </summary>
    /// <param name="entities">Entity list</param>
    /// <returns>Task</returns>
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
      var task = FindAsync();
      task.WaitSync();
      return task.Result;
    }

    /// <summary>
    /// Get a list of entities
    /// </summary>
    /// <returns>Query result</returns>
    public virtual async Task<IEnumerable<TEntity>> FindAsync()
    {
      return await FindAsync(null);
    }

    /// <summary>
    /// Filters a collection of entities using a predicate
    /// </summary>
    /// <param name="sql">SQL containing named parameter placeholders. For example: SELECT * FROM Customer WHERE Id = @Id</param>
    /// <param name="parameters">Named parameters</param>
    /// <param name="parameterPattern">Parameter Regex pattern, Defualts to @(\w+)</param>
    /// <returns>Filtered collection of entities</returns>
    public virtual IEnumerable<TEntity> Find(
      string sql,
      IDictionary<string, object> parameters = null,
      string parameterPattern = @"@(\w+)")
    {
      var task = FindAsync(sql, parameters, parameterPattern);
      task.WaitSync();
      return task.Result;
    }

    /// <summary>
    /// Filters a collection of entities using a predicate
    /// </summary>
    /// <param name="sql">SQL containing named parameter placeholders. For example: SELECT * FROM Customer WHERE Id = @Id</param>
    /// <param name="parameters">Named parameters</param>
    /// <param name="parameterPattern">Parameter Regex pattern, Defualts to @(\w+)</param>
    /// <returns>Filtered collection of entities</returns>
    public virtual async Task<IEnumerable<TEntity>> FindAsync(string sql, IDictionary<string, object> parameters = null, string parameterPattern = "@(\\w+)")
    {
      var query = DbContext.Set<TEntity>().AsQueryable();
      if (!string.IsNullOrWhiteSpace(sql))
      {
        if (parameters == null)
        {
          parameters = new Dictionary<string, object>();
        }

        var parameterValues = new List<object>();
        var placeholders = Regex.Matches(sql, parameterPattern);
        for (int i = 0; i < placeholders.Count; i++)
        {
          sql = sql.Replace(placeholders[i].Value, $"{{{i}}}");

          var parameterName = Regex.Match(placeholders[i].Value, @"(\w+)").Value;
          if (!parameters.ContainsKey(parameterName))
          {
            throw new ArgumentException($"Value must be specified for parameter \"{parameterName}\"");
          }

          parameterValues.Add(parameters[parameterName]);
        }

        query = query.FromSql(sql, parameterValues.ToArray());
      }

      foreach (var propertyName in Includes)
      {
        query = query.Include(propertyName);
      }

      if (CalculateTotalItems)
      {
        TotalItems = await query.LongCountAsync();
      }

      return await query
        .Sort(this)
        .Page(this)
        .ToListAsync();
    }

    /// <summary>
    /// Filters a collection of entities using a predicate
    /// </summary>
    /// <param name="where">Where predicate</param>
    /// <returns>Filtered collection of entities</returns>
    public virtual IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> where)
    {
      var task = FindAsync(where);
      task.WaitSync();
      return task.Result;
    }

    /// <summary>
    /// Filters a collection of entities using a predicate
    /// </summary>
    /// <param name="where">Where predicate</param>
    /// <returns>Filtered collection of entities</returns>
    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> where)
    {
      var query = DbContext.Set<TEntity>().AsQueryable();
      if (where != null)
      {
        query = query.Where(where);
      }

      foreach (var propertyName in Includes)
      {
        query = query.Include(propertyName);
      }

      if (CalculateTotalItems)
      {
        TotalItems = await query.LongCountAsync();
      }

      return await query
        .Sort(this)
        .Page(this)
        .ToListAsync();
    }

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="id">Filter to find a single item</param>
    /// <returns>Entity</returns>
    public virtual TEntity GetById(object id)
    {
      var task = GetByIdAsync(id);
      task.WaitSync();
      return task.Result;
    }

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="id">Filter to find a single item</param>
    /// <returns>Task</returns>
    public virtual async Task<TEntity> GetByIdAsync(object id)
    {
      IQueryable<TEntity> query = DbContext.Set<TEntity>();
      foreach (var propertyName in Includes)
      {
        query = query.Include(propertyName);
      }

      if (typeof(TEntity).GetProperty(IdPropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) == null)
      {
        return null;
      }

      ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "item");
      Expression whereProperty = Expression.Property(parameter, IdPropertyName);
      Expression constant = Expression.Constant(id);
      var converted = Expression.Convert(constant, whereProperty.Type);
      Expression condition = Expression.Equal(whereProperty, converted);
      Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(condition, parameter);
      return await query.SingleOrDefaultAsync(lambda);
    }

    /// <summary>
    /// Include referenced properties
    /// </summary>
    /// <param name="propertyPaths">Comma-separated list of property paths</param>
    /// <returns>Current instance</returns>
    IExpandableRepository<TEntity> IExpandableRepository<TEntity>.Include(string propertyPaths)
    {
      return Include(propertyPaths);
    }

    /// <summary>
    /// Include referenced properties
    /// </summary>
    /// <param name="propertyPaths">Comma-separated list of property paths</param>
    /// <returns>Current instance</returns>
    public IEntityFrameworkRepository<TEntity> Include(string propertyPaths)
    {
      if (!string.IsNullOrWhiteSpace(propertyPaths))
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
    IExpandableRepository<TEntity> IExpandableRepository<TEntity>.Include(
      Expression<Func<TEntity, object>> property)
    {
      return Include(property);
    }

    /// <summary>
    /// Include reference property
    /// </summary>
    /// <param name="property">Property expression</param>
    /// <returns>Current instance</returns>
    public IEntityFrameworkRepository<TEntity> Include(Expression<Func<TEntity, object>> property)
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
    IPageableRepository<TEntity> IPageableRepository<TEntity>.Page(int pageNumber, int pageSize)
    {
      return Page(pageNumber, pageSize);
    }

    /// <summary>
    /// Use paging
    /// </summary>
    /// <param name="pageNumber">Page to get (one based index).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>Current instance</returns>
    public IEntityFrameworkRepository<TEntity> Page(int pageNumber, int pageSize)
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
    ISortableRepository<TEntity> ISortableRepository<TEntity>.SortBy(
      Expression<Func<TEntity, object>> property)
    {
      return SortBy(property);
    }

    /// <summary>
    /// Property to sort by (ascending)
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>Current instance</returns>
    public IEntityFrameworkRepository<TEntity> SortBy(Expression<Func<TEntity, object>> property)
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
    ISortableRepository<TEntity> ISortableRepository<TEntity>.SortBy(string propertyName)
    {
      return SortBy(propertyName);
    }

    /// <summary>
    /// Sort ascending by a property
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    public IEntityFrameworkRepository<TEntity> SortBy(string propertyName)
    {
      if (!string.IsNullOrWhiteSpace(propertyName))
      {
        ValidatePropertyName(propertyName, out propertyName);
        SortOrder = Interfaces.SortOrder.Ascending;
        SortPropertyName = propertyName;
      }

      return this;
    }

    /// <summary>
    /// Property to sort by (descending)
    /// </summary>
    /// <param name="property">The property</param>
    /// <returns>Current instance</returns>
    ISortableRepository<TEntity> ISortableRepository<TEntity>.SortByDescending(
      Expression<Func<TEntity, object>> property)
    {
      return SortByDescending(property);
    }

    /// <summary>
    /// Property to sort by (descending)
    /// </summary>
    /// <param name="property">The property</param>
    /// <returns>Current instance</returns>
    public IEntityFrameworkRepository<TEntity> SortByDescending(Expression<Func<TEntity, object>> property)
    {
      if (property != null)
      {
        var name = GetPropertyName(property);
        SortByDescending(name);
      }

      return this;
    }

    /// <summary>
    /// Sort descending by a property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    ISortableRepository<TEntity> ISortableRepository<TEntity>.SortByDescending(string propertyName)
    {
      return SortByDescending(propertyName);
    }

    /// <summary>
    /// Sort descending by a property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    public IEntityFrameworkRepository<TEntity> SortByDescending(string propertyName)
    {
      if (!string.IsNullOrWhiteSpace(propertyName))
      {
        ValidatePropertyName(propertyName, out propertyName);
        SortOrder = Interfaces.SortOrder.Descending;
        SortPropertyName = propertyName;
      }

      return this;
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public virtual void Update(TEntity entity)
    {
      var task = UpdateAsync(entity);
      task.WaitSync();
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Task</returns>
    public virtual async Task UpdateAsync(TEntity entity)
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
    /// <returns>Current instance</returns>
    public virtual IRepository<TEntity> SaveChanges()
    {
      DbContext.SaveChanges();
      return this;
    }

    /// <summary>
    /// Persists all changes to the data storage
    /// <returns>Current instance</returns>
    /// </summary>
    /// <returns>Task</returns>
    public virtual async Task<IRepository<TEntity>> SaveChangesAsync()
    {
      await DbContext.SaveChangesAsync();
      return this;
    }

    /// <summary>
    /// Detaches all entites from the repository
    /// <returns>Current instance</returns>
    /// </summary>
    /// <returns>Current instance</returns>
    public virtual IRepository<TEntity> DetachAll()
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
    /// <returns>Task</returns>
    public virtual async Task<IRepository<TEntity>> DetachAllAsync()
    {
      await Task.Run(() => DetachAll());
      return this;
    }

    /// <summary>
    /// Gets a queryable collection of entities
    /// </summary>
    /// <returns>Queryable collection of entities</returns>
    public virtual IQueryable<TEntity> AsQueryable()
    {
      var query = DbContext.Set<TEntity>().AsQueryable();
      foreach (var propertyName in Includes)
      {
        query = query.Include(propertyName);
      }

      return query
        .Sort(this)
        .Page(this);
    }
  }
}
