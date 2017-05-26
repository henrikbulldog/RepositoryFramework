using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using RepositoryFramework.Interfaces;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using System.Reflection;

namespace RepositoryFramework.MongoDB
{
  /// <summary>
  /// Repository that uses the MongoDB document database, see https://docs.mongodb.com/
  /// </summary>
  /// <typeparam name="TEntity"></typeparam>
  public class MongoDBRepository<TEntity> : GenericRepositoryBase<TEntity>, IMongoDBRepository<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class
    /// </summary>
    /// <param name="MongoDB database">Database</param>
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
    /// MongoDB collection of entities/>
    /// </summary>
    protected virtual IMongoCollection<TEntity> Collection { get; private set; }

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
    public virtual async Task DeleteAsync(TEntity entity)
    {
      var builder = Builders<TEntity>.Filter;
      var filter = builder.Eq(IdPropertyName, entity.GetType().GetProperty(IdPropertyName).GetValue(entity));
      await Collection.DeleteOneAsync(filter);
    }

    /// <summary>
    /// Delete a list of existing entities
    /// </summary>
    /// <param name="entities">Entity list</param>
    public virtual void DeleteMany(IEnumerable<TEntity> list)
    {
      var builder = Builders<TEntity>.Filter;
      var filter = builder.In(IdPropertyName, list);
      Collection.DeleteMany(filter);
    }

    /// <summary>
    /// Delete a list of existing entities
    /// </summary>
    /// <param name="entities">Entity list</param>
    public virtual async Task DeleteManyAsync(IEnumerable<TEntity> list)
    {
      var builder = Builders<TEntity>.Filter;
      var filter = builder.In(IdPropertyName, list);
      await Collection.DeleteManyAsync(filter);
    }

    /// <summary>
    /// Finds a list of items using a filter expression
    /// </summary>
    /// <param name="expr">Filter expression</param>
    /// <returns>List of items</returns>
    public virtual IEnumerable<TEntity> Find()
    {
      return ApplyConstraints(Collection.Find("{}")).ToList();
    }

    /// <summary>
    /// Finds a list of items using a filter expression
    /// </summary>
    /// <param name="expr">Filter expression</param>
    /// <returns>List of items</returns>
    public virtual async Task<IEnumerable<TEntity>> FindAsync()
    {
      return await ApplyConstraints(Collection.Find("{}")).ToListAsync();
    }

    /// <summary>
    /// Filters a collection of entities using a predicate
    /// </summary>
    /// <param name="where">Where predicate</param>
    /// <returns>Filtered collection of entities</returns>
    public IEnumerable<TEntity> Find(Func<TEntity, bool> where)
    {
      return AsQueryable().Where(where).ToList();
    }

    /// <summary>
    /// Filters a collection of entities using a predicate
    /// </summary>
    /// <param name="where">Where predicate</param>
    /// <returns>Filtered collection of entities</returns>
    public virtual async Task<IEnumerable<TEntity>> FindAsync(Func<TEntity, bool> where)
    {
      return await Task.Run(() => AsQueryable().Where(where).ToList());
    }

    /// <summary>
    /// Filters a collection of entities
    /// </summary>
    /// <param name="filter">BSON filter definition</param>
    /// <returns>Filtered collection of entities</returns>
    public IEnumerable<TEntity> Find(string filter)
    {
      return ApplyConstraints(Collection.Find(filter)).ToList();
    }

    public async Task<IEnumerable<TEntity>> FindAsync(string filter)
    {
      return await ApplyConstraints(Collection.Find(filter)).ToListAsync();
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
      IQueryable<TEntity> query = Collection.AsQueryable();

      return query
        .Sort(this)
        .Page(this);
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
