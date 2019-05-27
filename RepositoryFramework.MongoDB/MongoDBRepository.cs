using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.MongoDB
{
  /// <summary>
  /// Repository that uses the MongoDB document database, see https://docs.mongodb.com/
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public class MongoDBRepository<TEntity> : GenericRepositoryBase<TEntity>, IMongoDBRepository<TEntity>
    where TEntity : class
  {
    private long totalItems = 0;
    private Task<long> totalItemsTask = null;
    private IQueryable<TEntity> whereExpression = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDBRepository{TEntity}"/> class
    /// </summary>
    /// <param name="database">MongoDB Database</param>
    /// <param name="classMapInitializer">Class map initializer</param>
    /// <param name="idProperty">Id property expression</param>
    public MongoDBRepository(
      IMongoDatabase database,
      Action<BsonClassMap<TEntity>> classMapInitializer,
      Expression<Func<TEntity, object>> idProperty = null)
      : base(idProperty)
    {
      if (classMapInitializer != null)
      {
        if (!BsonClassMap.IsClassMapRegistered(typeof(TEntity)))
        {
          try
          {
            BsonClassMap.RegisterClassMap(classMapInitializer);
          }
          catch
          {
          }
        }
      }

      Collection = database.GetCollection<TEntity>($"{EntityTypeName}Collection");
    }

    /// <summary>
    /// Gets the number of items per page (when paging is used)
    /// </summary>
    public virtual int PageSize { get; private set; } = 0;

    /// <summary>
    /// Gets the page number (one based index)
    /// </summary>
    public virtual int PageNumber { get; private set; } = 1;

    /// <summary>
    /// Gets the total number of items available in this set. For example, if a user has 100 blog posts, the response may only contain 10 items, but the totalItems would be 100.
    /// </summary>
    public virtual long TotalItems
    {
      get
      {
        if (this.totalItemsTask != null)
        {
          this.totalItemsTask.WaitSync();
          this.totalItems = this.totalItemsTask.Result;
          this.totalItemsTask = null;
        }

        return this.totalItems;
      }
    }

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
    public virtual SortOrder SortOrder { get; private set; } = SortOrder.Unspecified;

    /// <summary>
    /// Gets property name for the property to sort by.
    /// </summary>
    public virtual string SortPropertyName { get; private set; } = null;

    /// <summary>
    /// Gets the MongoDB collection of entities
    /// </summary>
    protected virtual IMongoCollection<TEntity> Collection { get; private set; }

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
    public IMongoDBRepository<TEntity> ClearPaging()
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
    public IMongoDBRepository<TEntity> ClearSorting()
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
      Collection.InsertOne(entity);
    }

    /// <summary>
    /// Create a new entity
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Task</returns>
    public virtual async Task CreateAsync(TEntity entity)
    {
      await Collection.InsertOneAsync(entity);
    }

    /// <summary>
    /// Create a list of new entities
    /// </summary>
    /// <param name="entities">List of entities</param>
    public virtual void CreateMany(IEnumerable<TEntity> entities)
    {
      Collection.InsertMany(entities);
    }

    /// <summary>
    /// Create a list of new entities
    /// </summary>
    /// <param name="entities">List of entities</param>
    /// <returns>Task</returns>
    public virtual async Task CreateManyAsync(IEnumerable<TEntity> entities)
    {
      await Collection.InsertManyAsync(entities);
    }

    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public virtual void Delete(TEntity entity)
    {
      var builder = Builders<TEntity>.Filter;
      var filter = builder.Eq(IdPropertyName, entity.GetType().GetProperty(IdPropertyName).GetValue(entity));
      Collection.DeleteOne(filter);
    }

    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Task</returns>
    public virtual async Task DeleteAsync(TEntity entity)
    {
      var builder = Builders<TEntity>.Filter;
      var filter = builder.Eq(IdPropertyName, entity.GetType().GetProperty(IdPropertyName).GetValue(entity));
      await Collection.DeleteOneAsync(filter);
    }

    /// <summary>
    /// Delete a list of existing entities
    /// </summary>
    /// <param name="list">Entity list</param>
    public virtual void DeleteMany(IEnumerable<TEntity> list)
    {
      var builder = Builders<TEntity>.Filter;
      var filter = builder.In(IdPropertyName, list);
      Collection.DeleteMany(filter);
    }

    /// <summary>
    /// Delete a list of existing entities
    /// </summary>
    /// <param name="list">Entity list</param>
    /// <returns>Task</returns>
    public virtual async Task DeleteManyAsync(IEnumerable<TEntity> list)
    {
      var builder = Builders<TEntity>.Filter;
      var filter = builder.In(IdPropertyName, list);
      await Collection.DeleteManyAsync(filter);
    }

    /// <summary>
    /// Finds a list of items using a filter expression
    /// </summary>
    /// <returns>List of items</returns>
    public virtual IEnumerable<TEntity> Find()
    {
      var task = FindAsync();
      task.WaitSync();
      return task.Result;
    }

    /// <summary>
    /// Finds a list of items using a filter expression
    /// </summary>
    /// <returns>List of items</returns>
    public virtual async Task<IEnumerable<TEntity>> FindAsync()
    {
      var r = await ApplyConstraints(Collection.Find("{}")).ToListAsync();
      if (PageSize == 0)
      {
        this.totalItems = r.LongCount();
      }
      else
      {
        this.totalItemsTask = Collection.CountAsync("{}");
      }

      return r;
    }

    /// <summary>
    /// Filters a collection of entities using a predicate
    /// </summary>
    /// <param name="where">Where predicate</param>
    /// <returns>Filtered collection of entities</returns>
    public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> where)
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
      var r = await ApplyConstraints(Collection.Find(where)).ToListAsync();
      if (PageSize == 0)
      {
        this.totalItems = r.LongCount();
      }
      else
      {
        this.totalItemsTask = Collection.CountAsync(where);
      }

      return r;
    }

    /// <summary>
    /// Filters a collection of entities
    /// </summary>
    /// <param name="filter">BSON filter definition</param>
    /// <returns>Filtered collection of entities</returns>
    public IEnumerable<TEntity> Find(string filter)
    {
      var task = FindAsync(filter);
      task.WaitSync();
      return task.Result;
    }

    /// <summary>
    /// Filters a collection of entities
    /// </summary>
    /// <param name="filter">BSON filter definition</param>
    /// <returns>Filtered collection of entities</returns>
    public async Task<IEnumerable<TEntity>> FindAsync(string filter)
    {
      var r = await ApplyConstraints(Collection.Find(filter)).ToListAsync();
      if (PageSize == 0)
      {
        this.totalItems = r.LongCount();
      }
      else
      {
        this.totalItemsTask = Collection.CountAsync(filter);
      }

      return r;
    }

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="id">Filter to find a single item</param>
    /// <returns>Entity</returns>
    public virtual TEntity GetById(object id)
    {
      var builder = Builders<TEntity>.Filter;
      var filter = builder.Eq(IdPropertyName, id);
      return Collection.Find(filter).FirstOrDefault();
    }

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="id">Filter to find a single item</param>
    /// <returns>Entity</returns>
    public virtual async Task<TEntity> GetByIdAsync(object id)
    {
      var builder = Builders<TEntity>.Filter;
      var filter = builder.Eq(IdPropertyName, id);
      return await Collection.Find(filter).FirstOrDefaultAsync();
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
    public IMongoDBRepository<TEntity> Page(int pageNumber, int pageSize)
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
    public IMongoDBRepository<TEntity> SortBy(Expression<Func<TEntity, object>> property)
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
    public IMongoDBRepository<TEntity> SortBy(string propertyName)
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
    public IMongoDBRepository<TEntity> SortByDescending(Expression<Func<TEntity, object>> property)
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
    ISortableRepository<TEntity> ISortableRepository<TEntity>.SortByDescending(string propertyName)
    {
      return SortByDescending(propertyName);
    }

    /// <summary>
    /// Sort descending by a property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Current instance</returns>
    public IMongoDBRepository<TEntity> SortByDescending(string propertyName)
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
      var builder = Builders<TEntity>.Filter;
      var filter = builder.Eq(IdPropertyName, entity.GetType().GetProperty(IdPropertyName).GetValue(entity));
      Collection.ReplaceOne(filter, entity);
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Task</returns>
    public virtual async Task UpdateAsync(TEntity entity)
    {
      var builder = Builders<TEntity>.Filter;
      var filter = builder.Eq(IdPropertyName, entity.GetType().GetProperty(IdPropertyName).GetValue(entity));
      await Collection.ReplaceOneAsync(filter, entity);
    }

    /// <summary>
    /// Gets a queryable collection of entities
    /// </summary>
    /// <returns>Queryable collection of entities</returns>
    public IQueryable<TEntity> AsQueryable()
    {
      if (whereExpression == null)
      {
        whereExpression = Collection.AsQueryable();
      }

      return whereExpression
        .Sort(this)
        .Page(this);
    }

    /// <inheritdoc/>
    public IQueryableRepository<TEntity> Where(Expression<Func<TEntity, bool>> where)
    {
      if (whereExpression == null)
      {
        whereExpression = Collection.AsQueryable();
      }

      whereExpression = whereExpression.Where(where);
      return this;
    }

    /// <inheritdoc/>
    public IQueryableRepository<TEntity> ClearWhere()
    {
      whereExpression = null;
      return this;
    }

    /// <summary>
    /// Applies sorting and paging constraints to a query
    /// </summary>
    /// <param name="findFluent">Find fluent query</param>
    /// <returns>Sorted and paged query</returns>
    protected virtual IFindFluent<TEntity, TEntity> ApplyConstraints(IFindFluent<TEntity, TEntity> findFluent)
    {
      if (SortOrder == SortOrder.Ascending)
      {
        findFluent = findFluent.SortBy(GetPropertySelector(SortPropertyName));
      }

      if (SortOrder == SortOrder.Descending)
      {
        findFluent = findFluent.SortByDescending(GetPropertySelector(SortPropertyName));
      }

      if (PageNumber > 1 || PageSize > 0)
      {
        findFluent = findFluent
          .Skip((PageNumber - 1) * PageSize)
          .Limit(PageSize);
      }

      return findFluent;
    }
  }
}
