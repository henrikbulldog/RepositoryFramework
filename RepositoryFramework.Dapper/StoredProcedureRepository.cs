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
  /// A Dapper repository using stored procedures
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  /// <typeparam name="TFilter">Filter type</typeparam>
  public class StoredProcedureRepository<TEntity, TFilter> : Repository<TEntity, TFilter>
    where TEntity : class
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="StoredProcedureRepository{TEntity, TFilter}"/> class
    /// </summary>
    /// <param name="connection">Database connection</param>
    /// <param name="entityIdProperty">Id property of the entity type</param>
    public StoredProcedureRepository(
      IDbConnection connection,
      Expression<Func<TEntity, object>> entityIdProperty = null)
      : base(connection, null, entityIdProperty)
    {
    }

    /// <summary>
    /// Create a new entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public override void Create(TEntity entity)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var columns = EntityColumns.Where(s => s != EntityIdPropertyName);
      var parameters = columns.Select(name => "@" + name ).ToList();

      IEnumerable<int> result = Connection.Query<int>(
        $"EXEC Create{EntityTypeName} {string.Join(",", parameters)}", 
        entity);

      EntityType.GetProperty(EntityIdPropertyName)?
        .SetValue(entity, result.First());
    }

    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public override void Delete(TEntity entity)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      Connection.Execute(
        $"EXEC Delete{EntityTypeName} @{EntityIdPropertyName}", 
        entity);
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public override void Update(TEntity entity)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      var parameters = EntityColumns.Select(name => "@" + name).ToList();

      Connection.Execute(
        $"EXEC Update{EntityTypeName} {string.Join(",", parameters)}",
        entity);
    }

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="filter">Filter</param>
    /// <returns>Entity</returns>
    public override TEntity GetById(string filter)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      IEnumerable<TEntity> result = Connection.Query<TEntity>(
        $"EXEC Get{EntityTypeName} @{EntityIdPropertyName}", 
        new { Id = filter });

      return result.FirstOrDefault();
    }

    /// <summary>
    /// Get a list of entities
    /// </summary>
    /// <returns>Query result</returns>
    public override IQueryResult<TEntity> Find()
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      IEnumerable<TEntity> result = Connection.Query<TEntity>(
        $"Find{EntityTypeName}",
        commandType: CommandType.StoredProcedure);

      return new QueryResult<TEntity>(result, result.Count());
    }

    /// <summary>
    /// Get a list of entites using an object with filtering information
    /// </summary>
    /// <param name="filter">Filter object</param>
    /// <returns>Query result</returns>
    public override IQueryResult<TEntity> Find(TFilter filter)
    {
      if (Connection.State != ConnectionState.Open)
      {
        Connection.Open();
      }

      IEnumerable<TEntity> result = Connection.Query<TEntity>(
        $"Find{EntityTypeName}",
        filter,
        commandType: CommandType.StoredProcedure);

      return new QueryResult<TEntity>(result, result.Count());
    }
  }
}
