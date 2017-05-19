using Dapper;
using RepositoryFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace RepositoryFramework.Dapper
{
  /// <summary>
  /// Repository that uses the Dapper micro-ORM framework, see https://github.com/StackExchange/Dapper
  /// </summary>
  /// <typeparam name="TEntity"></typeparam>
  public class DapperRepository<TEntity> :
    GenericRepositoryBase<TEntity>,
    IDapperRepository<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DapperRepository{TEntity}"/> class
    /// </summary>
    /// <param name="connection">Database connection</param>
    /// <param name="lastRowIdCommand">SQL command tpo get Id of the last row inserted. Defaults to TSQL syntax: SELECT @@IDENTITY</param>
    /// <param name="tableName">Table name. Defaults to entity type name</param>
    /// <param name="limitOffsetPattern">Limit and offset pattern. Must contain paceholders {PageNumber} and {PageSize}. Defaults to TSQL syntax: OFFSET ({PageNumber} - 1) * {PageSize} ROWS FETCH NEXT {PageSize} ROWS ONLY</param>
    public DapperRepository(
      IDbConnection connection,
      string lastRowIdCommand = "SELECT @@IDENTITY",
      string limitOffsetPattern = "OFFSET ({PageNumber} - 1) * {PageSize} ROWS FETCH NEXT {PageSize} ROWS ONLY",
      string tableName = null)
    {
      this.connection = connection;

      if (tableName != null)
      {
        TableName = tableName;
      }
      else
      {
        TableName = EntityTypeName;
      }

      LastRowIdCommand = lastRowIdCommand;
      LimitOffsetPattern = limitOffsetPattern;
    }

    private IDbConnection connection;

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
    /// Gets database connection
    /// </summary>
    protected virtual IDbConnection Connection
    {
      get
      {
        if (connection.State != ConnectionState.Open)
        {
          connection.Open();
        }
        return connection;
      }

      private set
      {
        connection = value;
      }
    }

    /// <summary>
    /// Gets SQL command to get Id of last row inserted
    /// </summary>
    protected virtual string LastRowIdCommand { get; private set; }

    /// <summary>
    /// Gets limit and offset pattern. Must contain paceholders {limit} and {offset}. Defaults to TSQL syntax: OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY
    /// </summary>
    protected virtual string LimitOffsetPattern { get; private set; }

    /// <summary>
    /// Gets table name
    /// </summary>
    protected virtual string TableName { get; private set; }

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
      CreateAsync(entity).WaitSync();
    }

    /// <summary>
    /// Create a new entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public virtual async Task CreateAsync(TEntity entity)
    {
      var insertColumns = EntityColumns.Where(c => c != IdPropertyName);

      var insertQuery = $@"
INSERT INTO {TableName} ({string.Join(",", insertColumns)})
VALUES (@{string.Join(",@", insertColumns)});
{LastRowIdCommand}";

      IEnumerable<int> result = await Connection.QueryAsync<int>(insertQuery, entity);
      EntityType.GetProperty(IdPropertyName)?
        .SetValue(entity, result.First());
    }

    /// <summary>
    /// Create a list of new entities
    /// </summary>
    /// <param name="entities">List of entities</param>
    public virtual void CreateMany(IEnumerable<TEntity> entities)
    {
      CreateManyAsync(entities).WaitSync();
    }

    /// <summary>
    /// Create a list of new entities
    /// </summary>
    /// <param name="entities">List of entities</param>
    public virtual async Task CreateManyAsync(IEnumerable<TEntity> entities)
    {
      var insertColumns = EntityColumns.Where(c => c != IdPropertyName);

      var insertCommand = $@"
INSERT INTO {TableName} ({string.Join(",", insertColumns)}) 
VALUES (@{string.Join(",@", insertColumns)})";

      await Connection.ExecuteAsync(insertCommand, entities.ToList());
    }

    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public virtual void Delete(TEntity entity)
    {
      DeleteAsync(entity).WaitSync();
    }

    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public virtual async Task DeleteAsync(TEntity entity)
    {
      var deleteQCommand = $@"
DELETE FROM {TableName}
WHERE {IdPropertyName}=@{IdPropertyName}";

      await Connection.ExecuteAsync(deleteQCommand, entity);
    }

    /// <summary>
    /// Delete a list of existing entities
    /// </summary>
    /// <param name="entities">Entity list</param>
    public virtual void DeleteMany(IEnumerable<TEntity> entities)
    {
      DeleteManyAsync(entities).WaitSync();
    }

    /// <summary>
    /// Delete a list of existing entities
    /// </summary>
    /// <param name="entities">Entity list</param>
    public virtual async Task DeleteManyAsync(IEnumerable<TEntity> entities)
    {
      var deleteCommand = $@"
DELETE FROM {TableName}
WHERE {IdPropertyName} IN (@Id)";

      var ids = new List<object>();
      foreach (var entity in entities)
      {
        ids.Add(EntityType.GetProperty(IdPropertyName).GetValue(entity));
      }

      await Connection.ExecuteAsync(deleteCommand, ids.Select(i => new { Id = i }).ToList());
    }

    /// <summary>
    /// Get a list of entities
    /// </summary>
    /// <returns>Query result</returns>
    public virtual IEnumerable<TEntity> Find()
    {
      return Connection.Query<TEntity>(GetQuery());
    }

    /// <summary>
    /// Get a list of entities
    /// </summary>
    /// <returns>Query result</returns>
    public virtual async Task<IEnumerable<TEntity>> FindAsync()
    {
      return await Connection.QueryAsync<TEntity>(GetQuery());
    }

    /// <summary>
    /// Filters a collection of entities from a filter definition
    /// </summary>
    /// <param name="filter">Filter definition</param>
    /// <returns>Filtered collection of entities</returns>
    public IEnumerable<TEntity> Find(string filter)
    {
      return Connection.Query<TEntity>(GetQuery(filter));
    }

    /// <summary>
    /// Filters a collection of entities from a filter definition
    /// </summary>
    /// <param name="filter">Filter definition</param>
    /// <returns>Filtered collection of entities</returns>
    public async Task<IEnumerable<TEntity>> FindAsync(string filter)
    {
      return await Connection.QueryAsync<TEntity>(GetQuery(filter));
    }
    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="id">Filter</param>
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
    /// <returns>Entity</returns>
    public virtual async Task<TEntity> GetByIdAsync(object id)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var findQuery = $@"
SELECT * FROM {TableName}
WHERE {IdPropertyName}=@{IdPropertyName}";

      return await Connection.QueryFirstOrDefaultAsync<TEntity>(findQuery, new { Id = id });
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
      UpdateAsync(entity).WaitSync();
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public virtual async Task UpdateAsync(TEntity entity)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var columns = EntityColumns.Where(s => s != IdPropertyName);
      var parameters = columns.Select(name => name + "=@" + name).ToList();

      var updateQuery = $@"
UPDATE {TableName} 
SET {string.Join(",", parameters)}
WHERE {IdPropertyName}=@{IdPropertyName}";

      await Connection.ExecuteAsync(updateQuery, entity);
    }

    /// <summary>
    /// Gets query string
    /// </summary>
    /// <param name="where">Optional where statement</param>
    /// <returns>Query string</returns>
    protected virtual string GetQuery(string where = null)
    {
      if (string.IsNullOrWhiteSpace(where))
      {
        where = string.Empty;
      }
      var orderBy = string.Empty;
      if (SortOrder != SortOrder.Unspecified
        && !string.IsNullOrWhiteSpace(SortPropertyName))
      {
        var order = SortOrder == SortOrder.Descending ? " DESC" : string.Empty;
        orderBy = $"ORDER BY {SortPropertyName}{order}";
      }

      var offset = string.Empty;
      if (PageNumber > 1 || PageSize > 0)
      {
        offset = LimitOffsetPattern.Replace("{PageNumber}", $"{PageNumber}").Replace("{PageSize}", $"{PageSize}");
      }

      return $@"
SELECT * FROM {TableName}
{where}
{orderBy}
{offset}";
    }
  }
}
