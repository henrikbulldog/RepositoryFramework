using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using RepositoryFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RepositoryFramework.MongoDB
{
  /// <summary>
  /// Repository for MongoDB.
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public class MongoRepository<TEntity> : IMongoRepository<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class
    /// <param name="MongoDB database">Database</param>
    /// <param name="classMapInitializer">Class map initializer</param>
    /// <param name="entityIdProperty">Id property of the entity type</param>
    /// </summary>
    public MongoRepository(
      IMongoDatabase database, 
      Action<BsonClassMap<TEntity>> classMapInitializer,
      Expression<Func<TEntity, object>> entityIdProperty = null)
    {
      if (classMapInitializer != null)
      {
        if (!BsonClassMap.IsClassMapRegistered(typeof(TEntity)))
        {
          BsonClassMap.RegisterClassMap(classMapInitializer);
        }
      }

      Collection = database.GetCollection<TEntity>($"{EntityTypeName}Collection");

      if (entityIdProperty != null)
      {
        EntityIdPropertyName = GetPropertyName(entityIdProperty);
      }
      else
      {
        EntityIdPropertyName = FindIdProperty();
      }
    }

    /// <summary>
    /// MongoDB collection of entities/>
    /// </summary>
    protected IMongoCollection<TEntity> Collection { get; private set;  }

    /// <summary>
    /// Gets entity type name
    /// </summary>
    protected string EntityTypeName { get; private set; } = typeof(TEntity).Name;

    /// <summary>
    /// Gets entity type
    /// </summary>
    protected Type EntityType { get; private set; } = typeof(TEntity);

    /// <summary>
    /// Gets entity Id property used to identify the resource in the path, for example Post.Id for /posts/{id}
    /// </summary>
    protected string EntityIdPropertyName { get; private set; }

    /// <summary>
    /// Create a new entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public void Create(TEntity entity)
    {
      Collection.InsertOne(entity);
    }

    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public void Delete(TEntity entity)
    {
      var builder = Builders<TEntity>.Filter;
      var filter = builder.Eq(EntityIdPropertyName, entity.GetType().GetProperty(EntityIdPropertyName).GetValue(entity));
      Collection.DeleteOne(filter);
    }

    /// <summary>
    /// Finds a list of entites using a filter expression and query constraints for expansion, paging and sorting.
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <returns>Current instance</returns>
    public IQueryResult<TEntity> Find(
      Expression<Func<TEntity, bool>> filter = null,
      QueryConstraints<TEntity> constraints = null)
    {
      IQueryable<TEntity> query = Collection.AsQueryable();

      if (filter != null)
      {
        query = query.Where(filter);
      }

      if (constraints != null)
      {
          query = AddSorting(constraints, query);
          query = AddPaging(constraints, query);
      }
      var items = query.ToList();

      var count = items.Count();

      return new QueryResult<TEntity>(items, count);
    }

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="filter">Entity id value</param>
    /// <returns>Entity</returns>
    public TEntity GetById(object filter)
    {
      var builder = Builders<TEntity>.Filter;
      var mongofilter = builder.Eq(EntityIdPropertyName, filter);
      return Collection.Find(mongofilter).FirstOrDefault();
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public void Update(TEntity entity)
    {
      var builder = Builders<TEntity>.Filter;
      var filter = builder.Eq(EntityIdPropertyName, entity.GetType().GetProperty(EntityIdPropertyName).GetValue(entity));
      Collection.ReplaceOne(filter, entity);
    }

    /// <summary>
    /// Get the name of a property from an expression
    /// </summary>
    /// <param name="propertyExpression">Property expression</param>
    /// <returns>Property name</returns>
    protected string GetPropertyName(Expression<Func<TEntity, object>> propertyExpression)
    {
      var body = propertyExpression.Body as MemberExpression;

      if (body != null)
      {
        return body.Member.Name;
      }

      var ubody = (UnaryExpression)propertyExpression.Body;
      body = ubody.Operand as MemberExpression;

      return body?.Member.Name ?? string.Empty;
    }

    /// <summary>
    /// Find the Id property of the entity type looking for properties with name Id or (entity type name)Id
    /// </summary>
    /// <returns>Id property name or null if none could befound</returns>
    protected string FindIdProperty()
    {
      var properties = EntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
      var idProperty = properties
        .FirstOrDefault(p => p.Name.ToLower() == $"{EntityTypeName.ToLower()}id");
      if (idProperty == null)
      {
        idProperty = properties
          .FirstOrDefault(p => p.Name.ToLower() == "id");
      }

      if (idProperty == null)
      {
        return null;
      }

      return idProperty.Name;
    }

    /// <summary>
    /// Add sorting constraint to a queryable expression
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="instance">Sortable instance</param>
    /// <param name="query">Queryable expression</param>
    /// <returns>Queryable expression with sortable constraint</returns>
    private IQueryable<TEntity> AddSorting(
      QueryConstraints<TEntity> instance,
      System.Linq.IQueryable<TEntity> query)
    {
      if (instance == null)
      {
        throw new ArgumentNullException(nameof(instance));
      }

      if (query == null)
      {
        throw new ArgumentNullException(nameof(query));
      }

      if (!string.IsNullOrEmpty(instance.SortPropertyName))
      {
        query = instance.SortOrder == SortOrder.Ascending
                    ? OrderBy(query, instance.SortPropertyName)
                    : OrderByDescending(query, instance.SortPropertyName);
      }

      return query;
    }

    /// <summary>
    /// Apply ordering to a LINQ query
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="source">Linq query</param>
    /// <param name="propertyName">Property to sort by</param>
    /// <returns>Ordered query</returns>
    private IQueryable<TEntity> OrderBy(
      System.Linq.IQueryable<TEntity> source,
      string propertyName)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      if (propertyName == null)
      {
        throw new ArgumentNullException(nameof(propertyName));
      }

      var type = typeof(TEntity);
      var property = type.GetProperty(propertyName);
      var parameter = Expression.Parameter(type, "p");
      var propertyAccess = Expression.MakeMemberAccess(parameter, property);
      var orderByExp = Expression.Lambda(propertyAccess, parameter);
      var resultExp = Expression.Call(
        typeof(Queryable),
        "OrderBy",
        new[] { type, property.PropertyType },
        source.Expression,
        Expression.Quote(orderByExp));
      return source.Provider.CreateQuery<TEntity>(resultExp);
    }

    /// <summary>
    /// Apply ordering to a LINQ query
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="source">Linq query</param>
    /// <param name="propertyName">Property to sort by</param>
    /// <returns>Ordered query</returns>
    private IQueryable<TEntity> OrderByDescending(
      System.Linq.IQueryable<TEntity> source,
      string propertyName)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      if (propertyName == null)
      {
        throw new ArgumentNullException(nameof(propertyName));
      }

      var type = typeof(TEntity);
      var property = type.GetProperty(propertyName);
      var parameter = Expression.Parameter(type, "p");
      var propertyAccess = Expression.MakeMemberAccess(parameter, property);
      var orderByExp = Expression.Lambda(propertyAccess, parameter);
      var resultExp = Expression.Call(
        typeof(Queryable),
        "OrderByDescending",
        new[] { type, property.PropertyType },
        source.Expression,
        Expression.Quote(orderByExp));
      return source.Provider.CreateQuery<TEntity>(resultExp);
    }

    /// <summary>
    /// Add paging to a queryable expression
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="instance">Pageable instance</param>
    /// <param name="query">Queryable expression</param>
    /// <returns>Queryable expression weith paging</returns>
    private IQueryable<TEntity> AddPaging(
      QueryConstraints<TEntity> instance,
      System.Linq.IQueryable<TEntity> query)
    {
      if (instance == null)
      {
        throw new ArgumentNullException(nameof(instance));
      }

      if (query == null)
      {
        throw new ArgumentNullException(nameof(query));
      }

      if (instance.PageNumber >= 1)
      {
        query = query.Skip((instance.PageNumber - 1) * instance.PageSize)
          .Take(instance.PageSize);
      }

      return query;
    }
  }
}
