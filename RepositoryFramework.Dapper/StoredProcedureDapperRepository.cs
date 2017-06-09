using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.Dapper
{
  /// <summary>
  /// A Dapper repository using stored procedures
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public class StoredProcedureDapperRepository<TEntity> :
    GenericRepositoryBase<TEntity>,
    IStoredProcedureDapperRepository<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="StoredProcedureDapperRepository{TEntity}"/> class
    /// </summary>
    /// <param name="connection">Database connection</param>
    /// <param name="idProperty">Id property expression</param>
    public StoredProcedureDapperRepository(
      IDbConnection connection,
      Expression<Func<TEntity, object>> idProperty = null)
      : base(idProperty)
    {
      Connection = connection;
    }

    /// <summary>
    /// Gets parameters
    /// </summary>
    public IDictionary<string, object> Parameters { get; private set; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets database connection
    /// </summary>
    protected virtual IDbConnection Connection { get; private set; }

    /// <summary>
    /// Clear parameters
    /// </summary>
    /// <returns>Current instance</returns>
    IParameterizedRepository<TEntity> IParameterizedRepository<TEntity>.ClearParameters()
    {
      return ClearParameters();
    }

    /// <summary>
    /// Clear parameters
    /// </summary>
    /// <returns>Current instance</returns>
    public IStoredProcedureDapperRepository<TEntity> ClearParameters()
    {
      Parameters.Clear();
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
    /// <returns>Task</returns>
    public virtual async Task CreateAsync(TEntity entity)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var parameters = EntityColumns
        .Where(c => c != IdPropertyName)
        .Select(c => $"@{c}=@{c}").ToList();

      var createCommand = $"EXEC Create{EntityTypeName} {string.Join(",", parameters)}";

      IEnumerable<int> result = await Connection.QueryAsync<int>(
        createCommand,
        entity);

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
    /// <returns>Task</returns>
    public virtual async Task CreateManyAsync(IEnumerable<TEntity> entities)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var parameters = EntityColumns
        .Where(c => c != IdPropertyName)
        .Select(c => "@" + c).ToList();

      await Connection.ExecuteAsync(
        $"EXEC Create{EntityTypeName} {string.Join(",", parameters)}",
        entities);
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
    /// <returns>Task</returns>
    public virtual async Task DeleteAsync(TEntity entity)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      await Connection.ExecuteAsync(
        $"EXEC Delete{EntityTypeName} @{IdPropertyName}",
        entity);
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
    /// <returns>Task</returns>
    public virtual async Task DeleteManyAsync(IEnumerable<TEntity> entities)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      await Connection.ExecuteAsync(
        $@"EXEC Delete{EntityTypeName} @{IdPropertyName}",
        entities);
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
    /// <returns>Task</returns>
    public virtual async Task UpdateAsync(TEntity entity)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var parameters = EntityColumns.Select(name => $"@{name}=@{name}").ToList();

      await Connection.ExecuteAsync(
        $"EXEC Update{EntityTypeName} {string.Join(",", parameters)}",
        entity);
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

      IEnumerable<TEntity> result = await Connection.QueryAsync<TEntity>(
        $"EXEC Get{EntityTypeName} @{IdPropertyName}",
        new { Id = id });

      return result.FirstOrDefault();
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
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var parameters = Parameters.Keys
        .Select(key => $"@{key}=@{key}");

      return await Connection.QueryAsync<TEntity>(
        $"EXEC Find{EntityTypeName} {string.Join(",", parameters)}",
        ToObject(Parameters));
    }

    /// <summary>
    /// Gets parameter value
    /// </summary>
    /// <param name="name">Parameter name</param>
    /// <returns>Parameter value</returns>
    public object GetParameter(string name)
    {
      return Parameters[name];
    }

    /// <summary>
    /// Adds a parameter to queries
    /// </summary>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Parameter value</param>
    /// <returns>Current instance</returns>
    IParameterizedRepository<TEntity> IParameterizedRepository<TEntity>.SetParameter(string name, object value)
    {
      return SetParameter(name, value);
    }

    /// <summary>
    /// Adds a parameter to queries
    /// </summary>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Parameter value</param>
    /// <returns>Current instance</returns>
    public IStoredProcedureDapperRepository<TEntity> SetParameter(string name, object value)
    {
      if (!Parameters.Keys.Contains(name))
      {
        Parameters.Add(name, value);
      }
      else
      {
        Parameters[name] = value;
      }

      return this;
    }
  }
}
