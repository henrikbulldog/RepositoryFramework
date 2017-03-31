using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RepositoryFramework.Interfaces;
using RestSharp;

namespace RepositoryFramework.Api
{
  /// <summary>
  /// Repository abstraction of a ReSTful API
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  /// <typeparam name="TFindFilter">Filter type</typeparam>
  public class ApiFilterRepository<TEntity, TFindFilter> :
    ApiRepository<TEntity>, IFindFilter<TEntity, TFindFilter>
    where TEntity : class
    where TFindFilter : class
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiFilterRepository{TEntity, TFindFilter}" /> class.
    /// </summary>
    /// <param name="configuration">Configuration</param>
    /// <param name="basePath">The base path.</param>
    /// <param name="entityPath">Path excluding the base path. May contain path paramater placeholders in the format {someParm}</param>
    /// <param name="entityIdProperty">Entity Id property expression</param>
    public ApiFilterRepository(
      Configuration configuration,
      string basePath,
      string entityPath = null,
      Expression<Func<TEntity, object>> entityIdProperty = null)
      : base(configuration, basePath, entityPath, entityIdProperty)
    {
    }

    /// <summary>
    /// Get a list of entites using an object with filtering information
    /// </summary>
    /// <param name="filter">Filter object</param>
    /// <returns>Query result</returns>
    public virtual IQueryResult<TEntity> Find(TFindFilter filter)
    {
      var path = EntityPath;
      path = path.Replace("{format}", "json");

      var pathParams = new Dictionary<string, string>();
      var queryParams = new Dictionary<string, string>();
      var headerParams = new Dictionary<string, string>();
      var formParams = new Dictionary<string, string>();
      var fileParams = new Dictionary<string, FileParameter>();
      string postBody = null;

      if (filter != null)
      {
        foreach (string filterProperty in filter
          .GetType()
          .GetProperties(BindingFlags.Public | BindingFlags.Instance)
          .Select(p => p.Name))
        {
          var parameterValue = ParameterToString(filter.GetType().GetProperty(filterProperty).GetValue(filter));
          var pathParmeter = PathParameters.FirstOrDefault(p => p.ToLower() == filterProperty.ToLower());
          if (pathParmeter != null)
          {
            if (parameterValue == null)
            {
              throw new Exception($"Path parameter {pathParmeter} cannot be null");
            }

            pathParams.Add(pathParmeter, parameterValue);
          }
          else
          {
            if (parameterValue != null)
            {
              queryParams.Add(FirstCharacterToLower(filterProperty), parameterValue);
            }
          }
        }
      }

      IRestResponse response = (IRestResponse)CallApi(
        path,
        Method.GET,
        pathParams,
        queryParams,
        postBody,
        headerParams,
        formParams,
        fileParams);

      if (((int)response.StatusCode) >= 400)
      {
        throw new ApiException(
          (int)response.StatusCode,
          $"Error calling {EntityType}Repository.Find(): {response.Content}",
          "GET",
          BasePath,
          path,
          filter,
          null,
          response.Content);
      }
      else if (((int)response.StatusCode) == 0)
      {
        throw new ApiException(
          (int)response.StatusCode,
          $"Error calling {EntityType}Repository.Find(): {response.ErrorMessage}",
          "GET",
          BasePath,
          path,
          filter,
          null,
          response.ErrorMessage);
      }

      var result = (List<TEntity>)Deserialize(response.Content, typeof(List<TEntity>), response.Headers);
      return new QueryResult<TEntity>(result, result.Count);
    }
  }
}