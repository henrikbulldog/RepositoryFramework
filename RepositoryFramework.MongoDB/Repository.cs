using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using RepositoryFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace RepositoryFramework.MongoDB
{
  public class Repository<TEntity> : IRepository<TEntity>, IFindQueryable<TEntity>, IGetQueryable<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class
    /// </summary>
    public Repository(IMongoDatabase database, Action<BsonClassMap<TEntity>> classMapInitializer)
    {
      if (classMapInitializer != null)
      {
        if (!BsonClassMap.IsClassMapRegistered(typeof(TEntity)))
        {
          BsonClassMap.RegisterClassMap(classMapInitializer);
        }
      }

      Collection = database.GetCollection<TEntity>($"{EntityTypeName}Collection");
    }

    /// <summary>
    /// MongoDB collection of entities/>
    /// </summary>
    protected IMongoCollection<TEntity> Collection { get; private set;  }

    /// <summary>
    /// Gets entity type name
    /// </summary>
    protected string EntityTypeName { get; private set; } = typeof(TEntity).Name;


    public void Create(TEntity entity)
    {
      Collection.InsertOne(entity);
    }

    public void Delete(TEntity entity)
    {
      throw new NotImplementedException();
    }

    public IQueryResult<TEntity> Find()
    {
      return Find(null, null);
    }

    public IQueryResult<TEntity> Find(IQueryConstraints<TEntity> constraints)
    {
      return Find(null, constraints);
    }

    public IQueryResult<TEntity> Find(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
    {
      return Find(filter, null);
    }

    public IQueryResult<TEntity> Find(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter, IQueryConstraints<TEntity> constraints)
    {
      var query = Filter(filter);

      IEnumerable<TEntity> items;

      if (constraints != null)
      {
        items = ApplyQueryConstraints(constraints, query).ToList();
      }
      else
      {
        items = query.ToList();
      }

      var count = query.Count();

      return new QueryResult<TEntity>(items, count);
    }

    public TEntity GetById(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
    {
      throw new NotImplementedException();
    }

    public TEntity GetById(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter, IExpandable<TEntity> includes)
    {
      throw new NotImplementedException();
    }

    public void Update(TEntity entity)
    {
      throw new NotImplementedException();
    }

    private IQueryable<TEntity> Filter(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
    {
      IQueryable<TEntity> query;

      if (filter != null)
      {
        query = filter(Collection.AsQueryable());
      }
      else
      {
        query = Collection.AsQueryable();
      }

      return query;
    }

    private IQueryable<TEntity> ApplyQueryConstraints(
      IQueryConstraints<TEntity> instance,
      IQueryable<TEntity> query)
    {
      if (instance == null)
      {
        throw new ArgumentNullException(nameof(instance));
      }

      if (query == null)
      {
        throw new ArgumentNullException(nameof(query));
      }

      query = AddSorting(instance.Sortable, query);
      query = AddPaging(instance.Pageable, query);
      query = AddExpansion(instance.Expandable, query);

      return query;
    }

    /// <summary>
    /// Add sorting constraint to a queryable expression
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="instance">Sortable instance</param>
    /// <param name="query">Queryable expression</param>
    /// <returns>Queryable expression with sortable constraint</returns>
    private IQueryable<TEntity> AddSorting(
      ISortable<TEntity> instance,
      IQueryable<TEntity> query)
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
      IQueryable<TEntity> source,
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
      IQueryable<TEntity> source,
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
      IPageable<TEntity> instance,
      IQueryable<TEntity> query)
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

    /// <summary>
    /// Apply expansion to LINQ statement
    /// </summary>
    /// <typeparam name="TEntity">Model type</typeparam>
    /// <param name="instance">Expandable instance</param>
    /// <param name="query">Queryable expression</param>
    /// <returns>Queryable expression with expansion</returns>
    private IQueryable<TEntity> AddExpansion(
      IExpandable<TEntity> instance,
      IQueryable<TEntity> query)
    {
      if (instance == null)
      {
        throw new ArgumentNullException(nameof(instance));
      }

      if (query == null)
      {
        throw new ArgumentNullException(nameof(query));
      }

      if (instance.Includes != null)
      {
        foreach (var propertyName in instance.Includes)
        {
          query = Include(query, propertyName);
        }
      }

      return query;
    }

    private IQueryable<TEntity> Include(IQueryable<TEntity> query, string navigationPropertyPath)
    {
      // TODO: Can expansion be supported in MongoDB?
      return query;
    }

  }
}
