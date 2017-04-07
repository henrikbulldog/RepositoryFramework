using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapper;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.Dapper
{
  /// <summary>
  /// A Dapper repository
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  /// <typeparam name="TFilter">Filter type</typeparam>
  public class Repository<TEntity, TFilter> :
    IRepository<TEntity>,
    IFind<TEntity>,
    IGet<TEntity, string>,
    IGet<TEntity, int>,
    IGet<TEntity, Guid>,
    IFindFilter<TEntity, TFilter>
    where TEntity : class
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity, TFilter}"/> class
    /// </summary>
    /// <param name="connection">Database connection</param>
    /// <param name="lastRowIdCommand">SQL command tpo get Id of the last row inserted</param>
    /// <param name="entityIdProperty">Id property of the entity type</param>
    public Repository(
      IDbConnection connection,
      string lastRowIdCommand = "SELECT @@IDENTITY",
      Expression<Func<TEntity, object>> entityIdProperty = null)
    {
      this.Connection = connection;

      this.LastRowIdCommand = lastRowIdCommand;

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
    /// Gets entity type
    /// </summary>
    protected Type EntityType { get; private set; } = typeof(TEntity);

    /// <summary>
    /// Gets entity type name
    /// </summary>
    protected string EntityTypeName { get; private set; } = typeof(TEntity).Name;

    /// <summary>
    /// Gets entity database columns
    /// </summary>
    protected string[] EntityColumns { get; private set; } = typeof(TEntity).GetProperties()
      .Where(p => (p.PropertyType.GetTypeInfo().GetInterface("IEnumerable") == null
      && p.PropertyType.GetTypeInfo().GetInterface("ICollection") == null
      && !p.PropertyType.GetTypeInfo().IsClass)
      || p.PropertyType.IsAssignableFrom(typeof(string)))
      .Select(p => p.Name)
      .ToArray();

    /// <summary>
    /// Gets entity database columns
    /// </summary>
    protected string[] FilterColumns { get; private set; } = typeof(TFilter).GetProperties()
      .Where(p => (p.PropertyType.GetTypeInfo().GetInterface("IEnumerable") == null
      && p.PropertyType.GetTypeInfo().GetInterface("ICollection") == null
      && !p.PropertyType.GetTypeInfo().IsClass)
      || p.PropertyType.IsAssignableFrom(typeof(string)))
      .Select(p => p.Name)
      .ToArray();

    /// <summary>
    /// Gets database connection
    /// </summary>
    protected IDbConnection Connection { get; private set; }

    /// <summary>
    /// Gets table name
    /// </summary>
    protected string TableName { get; private set; } = typeof(TEntity).Name;

    /// <summary>
    /// Gets SQL command to get Id of last row inserted
    /// </summary>
    protected string LastRowIdCommand { get; private set; }

    /// <summary>
    /// Gets entity Id property used to identify the resource in the path, for example Post.Id for /posts/{id}
    /// </summary>
    protected string EntityIdPropertyName { get; private set; }

    /// <summary>
    /// Create a new entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public virtual void Create(TEntity entity)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var insertQuery = $@"
INSERT INTO {TableName} ({string.Join(",", EntityColumns)})
VALUES (@{string.Join(",@", EntityColumns)});
{LastRowIdCommand}";

      IEnumerable<int> result = Connection.Query<int>(insertQuery, entity);

      EntityType.GetProperty(EntityIdPropertyName)?
        .SetValue(entity, result.First());
    }

    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public virtual void Delete(TEntity entity)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var deleteQuery = $@"
DELETE FROM {TableName}
WHERE {EntityIdPropertyName}=@{EntityIdPropertyName}";

      Connection.Execute(deleteQuery, entity);
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public virtual void Update(TEntity entity)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var columns = EntityColumns.Where(s => s != EntityIdPropertyName);
      var parameters = columns.Select(name => name + "=@" + name).ToList();

      var updateQuery = $@"
UPDATE {TableName} 
SET {string.Join(",", parameters)}
WHERE {EntityIdPropertyName}=@{EntityIdPropertyName}";

      Connection.Execute(updateQuery, entity);
    }

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="filter">Filter</param>
    /// <returns>Entity</returns>
    public virtual TEntity GetById(string filter)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var findQuery = $@"
SELECT * FROM {TableName}
WHERE {EntityIdPropertyName}=@{EntityIdPropertyName}";

      IEnumerable<TEntity> result = Connection.Query<TEntity>(findQuery, new { Id = filter });

      return result.FirstOrDefault();
    }

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="filter">Filter</param>
    /// <returns>Entity</returns>
    public virtual TEntity GetById(Guid filter)
    {
      return GetById(filter.ToString());
    }

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="filter">Filter</param>
    /// <returns>Entity</returns>
    public virtual TEntity GetById(int filter)
    {
      return GetById(filter.ToString());
    }

    /// <summary>
    /// Get a list of entities
    /// </summary>
    /// <returns>Query result</returns>
    public virtual IQueryResult<TEntity> Find()
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var findQuery = $@"
SELECT * FROM {TableName}";

      IEnumerable<TEntity> result = Connection.Query<TEntity>(findQuery);

      return new QueryResult<TEntity>(result, result.Count());
    }

    /// <summary>
    /// Get a list of entites using an object with filtering information
    /// </summary>
    /// <param name="filter">Filter object</param>
    /// <returns>Query result</returns>
    public virtual IQueryResult<TEntity> Find(TFilter filter)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var findQuery = $@"
SELECT * FROM {TableName}
{CreateWhere(filter)}";

      IEnumerable<TEntity> result = Connection.Query<TEntity>(findQuery, filter);

      return new QueryResult<TEntity>(result, result.Count());
    }

    /// <summary>
    /// Create SQL where clause
    /// </summary>
    /// <param name="filter">Filter object</param>
    /// <returns>SQL where statement</returns>
    protected string CreateWhere(TFilter filter)
    {
      if (filter == null)
      {
        return string.Empty;
      }

      var parameters = new List<string>();
        foreach (var columnName in FilterColumns)
        {
          if (typeof(TFilter).GetProperty(columnName).GetValue(filter) != null)
          {
            parameters.Add($"{TableName}.{columnName}=@{columnName}");
          }
        }

      if (parameters.Count == 0)
      {
        return string.Empty;
      }

      return $"WHERE {string.Join(" AND ", parameters)}";
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
    protected virtual string FindIdProperty()
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
  }
}
