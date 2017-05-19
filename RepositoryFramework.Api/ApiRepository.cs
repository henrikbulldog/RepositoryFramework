using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RepositoryFramework.Interfaces;
using RestSharp;
using System.Reflection;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.IO;
using RestSharp.Extensions;
using Newtonsoft.Json;
using RestSharp.Authenticators;

namespace RepositoryFramework.Api
{
  /// <summary>
  /// Repository abstraction of a ReSTful API
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public class ApiRepository<TEntity> :
    GenericRepositoryBase<TEntity>,
    IApiRepository<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiRepository{TEntity}" /> class.
    /// </summary>
    /// <param name="configuration">Configuration</param>
    /// <param name="basePath">The base path.</param>
    /// <param name="entityPath">Path excluding the base path. May contain path paramater placeholders in the format {someParm}</param>
    public ApiRepository(
      Configuration configuration,
      string basePath,
      string entityPath = null)
    {
      this.Configuration = configuration;
      BasePath = basePath;
      RestClient = new RestClient(BasePath);

      if (string.IsNullOrEmpty(entityPath))
      {
        EntityPath = $"/{FirstCharacterToLower(EntityTypeName)}s";
      }
      else
      {
        EntityPath = entityPath;
      }
    }

    /// <summary>
    /// Gets or the base path.
    /// </summary>
    /// <value>The base path</value>
    public string BasePath { get; private set; }

    /// <summary>
    /// Gets entity path, for example /posts in https://jsonplaceholder.typicode.com/posts
    /// </summary>
    protected string EntityPath { get; private set; }

    /// <summary>
    /// Gets configuration object
    /// </summary>
    protected Configuration Configuration { get; private set; } = null;

    /// <summary>
    /// Gets default header map
    /// </summary>
    protected Dictionary<string, string> DefaultHeaderMap { get; private set; }
      = new Dictionary<string, string>();

    /// <summary>
    /// Gets the RestClient.
    /// </summary>
    /// <value>An instance of the RestClient</value>
    protected RestClient RestClient { get; private set; }

    /// <summary>
    /// Gets the default header.
    /// </summary>
    protected Dictionary<string, string> DefaultHeader
    {
      get { return DefaultHeaderMap; }
    }

    /// <summary>
    /// Parameters
    /// </summary>
    public IDictionary<string, object> Parameters { get; private set; } = new Dictionary<string, object>();

    /// <summary>
    /// Clear parameters
    /// </summary>
    /// <returns>Current instance</returns>
    public IRepository<TEntity> ClearParameters()
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
    public virtual async Task CreateAsync(TEntity entity)
    {
      if (entity == null)
      {
        throw new ApiException(
          400,
          $"Missing required parameter entity when calling APIRepository<{EntityTypeName}>.Create()",
          "POST",
          EntityPath,
          null,
          entity);
      }

      var path = EntityPath;
      path = path.Replace("{format}", "json");

      var pathParams = GetPathParameters(path);
      var queryParams = GetQueryParameters(path);
      var headerParams = new Dictionary<string, string>();
      var formParams = new Dictionary<string, string>();
      var fileParams = new Dictionary<string, FileParameter>();
      string postBody = Serialize(entity);

      var response = await CallApiAsync(
          path,
          Method.POST,
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
          $"Error calling APIRepository<{EntityTypeName}>.Create: {response.Content}",
          "POST",
          BasePath,
          EntityPath,
          response.Content);
      }
      else if (((int)response.StatusCode) == 0)
      {
        throw new ApiException(
          (int)response.StatusCode,
          $"Error calling APIRepository<{EntityTypeName}>.Update: {response.ErrorMessage}",
          "POST",
          BasePath,
          EntityPath);
      }

      DeserializeAndPopulate(response.Content, ref entity, response.Headers);
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
      if (entities == null)
      {
        throw new ApiException(
          400,
          $"Missing required parameter entity when calling APIRepository<{EntityTypeName}>.Create()",
          "POST",
          EntityPath,
          null,
          entities);
      }

      var path = EntityPath;
      path = path.Replace("{format}", "json");

      var pathParams = GetPathParameters(path);
      var queryParams = GetQueryParameters(path);
      var headerParams = new Dictionary<string, string>();
      var formParams = new Dictionary<string, string>();
      var fileParams = new Dictionary<string, FileParameter>();
      string postBody = Serialize(entities);

      var task = CallApiAsync(
          path,
          Method.POST,
          pathParams,
          queryParams,
          postBody,
          headerParams,
          formParams,
          fileParams);

      var response = await task;

      if (((int)response.StatusCode) >= 400)
      {
        throw new ApiException(
          (int)response.StatusCode,
          $"Error calling APIRepository<{EntityTypeName}>.Create: {response.Content}",
          "POST",
          BasePath,
          EntityPath,
          response.Content);
      }
      else if (((int)response.StatusCode) == 0)
      {
        throw new ApiException(
          (int)response.StatusCode,
         $"Error calling APIRepository<{EntityTypeName}>.Update: {response.ErrorMessage}",
         "POST",
         BasePath,
         EntityPath);
      }

      DeserializeAndPopulate(response.Content, ref entities, response.Headers);
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
      if (entity == null)
      {
        throw new ApiException(
          400,
          $"Missing required parameter entity when calling APIRepository<{EntityTypeName}>.Update()",
          "DELETE",
          BasePath,
          EntityPath);
      }

      var path = EntityPath;
      var id = GetIdPropertyValue(entity);
      if (id != null)
      {
        path = $"{path}/{id}";
      }

      path = path.Replace("{format}", "json");
      var pathParams = GetPathParameters(path);
      var queryParams = GetQueryParameters(path);
      var headerParams = new Dictionary<string, string>();
      var formParams = new Dictionary<string, string>();
      var fileParams = new Dictionary<string, FileParameter>();
      string postBody = Serialize(entity);

      var task = CallApiAsync(
        path,
        Method.DELETE,
        pathParams,
        queryParams,
        postBody,
        headerParams,
        formParams,
        fileParams);
      var response = await task;

      if (((int)response.StatusCode) >= 400)
      {
        throw new ApiException(
          (int)response.StatusCode,
          $"Error calling APIRepository<{EntityTypeName}>.Delete: {response.Content}",
          "DELETE",
          BasePath,
          EntityPath,
          response.Content);
      }
      else if (((int)response.StatusCode) == 0)
      {
        throw new ApiException(
          (int)response.StatusCode,
         $"Error calling APIRepository<{EntityTypeName}>.Delete: {response.ErrorMessage}",
         "DELETE",
         EntityPath,
         response.Content);
      }
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
      foreach (var entity in entities)
      {
        await DeleteAsync(entity);
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
      var path = EntityPath;
      path = path.Replace("{format}", "json");

      var pathParams = GetPathParameters(path);
      var queryParams = GetQueryParameters(path);
      var headerParams = new Dictionary<string, string>();
      var formParams = new Dictionary<string, string>();
      var fileParams = new Dictionary<string, FileParameter>();
      string postBody = null;

      var task = CallApiAsync(
        path,
        Method.GET,
        pathParams,
        queryParams,
        postBody,
        headerParams,
        formParams,
        fileParams);
      var response = await task;

      if (((int)response.StatusCode) >= 400)
      {
        throw new ApiException(
          (int)response.StatusCode,
          $"Error calling {EntityType}Repository.Find(): {response.Content}",
          "GET",
          BasePath,
          path,
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
          response.ErrorMessage);
      }

      return (List<TEntity>)Deserialize(response.Content, typeof(List<TEntity>), response.Headers);
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
    /// <param name="id">Filter</param>
    /// <returns>Entity</returns>
    public virtual async Task<TEntity> GetByIdAsync(object id)
    {
      if (id == null)
      {
        throw new ApiException(
          400,
          $"Missing required parameter filter when calling APIRepository<{EntityTypeName}>.GetById()",
          "GET",
          BasePath,
          EntityPath,
          id);
      }

      var path = $"{EntityPath}/{id}";
      path = path.Replace("{format}", "json");

      var pathParams = GetPathParameters(path);
      var queryParams = GetQueryParameters(path);
      var headerParams = new Dictionary<string, string>();
      var formParams = new Dictionary<string, string>();
      var fileParams = new Dictionary<string, FileParameter>();
      string postBody = null;

      var task = CallApiAsync(
        path,
        Method.GET,
        pathParams,
        queryParams,
        postBody,
        headerParams,
        formParams,
        fileParams);
      var response = await task;

      if (((int)response.StatusCode) >= 400)
      {
        throw new ApiException(
          (int)response.StatusCode,
          $"Error calling APIRepository<{EntityTypeName}>.GetById: {response.Content}",
          "GET",
          BasePath,
          path,
          response.Content);
      }
      else if (((int)response.StatusCode) == 0)
      {
        throw new ApiException(
        (int)response.StatusCode,
          $"Error calling APIRepository<{EntityTypeName}>.GetById: {response.ErrorMessage}",
          "GET",
          BasePath,
          path,
          response.Content);
      }

      return (TEntity)Deserialize(response.Content, typeof(TEntity), response.Headers);
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
    public IRepository<TEntity> SetParameter(string name, object value)
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
      if (entity == null)
      {
        throw new ApiException(
          400,
          $"Missing required parameter entity when calling APIRepository<{EntityTypeName}>.Update()",
          "PUT",
          BasePath,
          EntityPath);
      }

      var path = EntityPath;
      var id = GetIdPropertyValue(entity);
      if (id != null)
      {
        path = $"{path}/{id}";
      }

      path = path.Replace("{format}", "json");

      var pathParams = GetPathParameters(path);
      var queryParams = GetQueryParameters(path);
      var headerParams = new Dictionary<string, string>();
      var formParams = new Dictionary<string, string>();
      var fileParams = new Dictionary<string, FileParameter>();
      string postBody = Serialize(entity);

      var task = CallApiAsync(
        path,
        Method.PUT,
        pathParams,
        queryParams,
        postBody,
        headerParams,
        formParams,
        fileParams);
      var response = await task;

      if (((int)response.StatusCode) >= 400)
      {
        throw new ApiException(
          (int)response.StatusCode,
          $"Error calling APIRepository<{EntityTypeName}>.Update: {response.Content}",
          "PUT",
          BasePath,
          path,
          response.Content);
      }
      else if (((int)response.StatusCode) == 0)
      {
        throw new ApiException(
          (int)response.StatusCode,
          $"Error calling APIRepository<{EntityTypeName}>.Update: {response.ErrorMessage}",
          "PUT",
          BasePath,
          path,
          response.Content);
      }

      DeserializeAndPopulate(response.Content, ref entity, response.Headers);
    }

    /// <summary>
    /// Gets path parameters
    /// </summary>
    /// <param name="path">Path</param>
    /// <returns>Path parameters</returns>
    protected Dictionary<string, string> GetPathParameters(string path)
    {
      var pathParameters = new Dictionary<string, string>();
      if (Parameters.Count != 0)
      {
        var pathParameterNames = GetParameterNamesFromPath(path);
        foreach (string parameter in Parameters.Keys)
        {
          var pathParmeterName = pathParameterNames.FirstOrDefault(p => p.ToLower() == parameter.ToLower());
          if (pathParmeterName != null)
          {
            pathParameters.Add(pathParmeterName, ParameterToString(Parameters[parameter]));
          }
        }
      }
      return pathParameters;
    }

    /// <summary>
    /// Gets query parameters
    /// </summary>
    /// <param name="path">Path</param>
    /// <returns>Query parameters</returns>
    protected Dictionary<string, string> GetQueryParameters(string path)
    {
      var queryParameters = new Dictionary<string, string>();
      if (Parameters.Count != 0)
      {
        var pathParameterNames = GetParameterNamesFromPath(path);
        foreach (string parameter in Parameters.Keys)
        {
          if (!pathParameterNames.Exists(p => p.ToLower() == parameter.ToLower()))
          {
            if (Parameters[parameter] != null)
            {
              queryParameters.Add(FirstCharacterToLower(parameter), ParameterToString(Parameters[parameter]));
            }
          }
        }
      }
      return queryParameters;
    }

    /// <summary>
    /// Add default header.
    /// </summary>
    /// <param name="key">Header field name.</param>
    /// <param name="value">Header field value.</param>
    protected void AddDefaultHeader(string key, string value)
    {
      DefaultHeaderMap.Add(key, value);
    }

    /// <summary>
    /// Escape string (url-encoded).
    /// </summary>
    /// <param name="str">String to be escaped.</param>
    /// <returns>Escaped string.</returns>
    protected string EscapeString(string str)
    {
      return RestSharp.Extensions.StringExtensions.UrlEncode(str);
    }

    /// <summary>
    /// Create FileParameter based on Stream.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="stream">Input stream.</param>
    /// <returns>FileParameter.</returns>
    protected FileParameter ParameterToFile(string name, Stream stream)
    {
      if (stream is FileStream)
      {
        return FileParameter.Create(name, stream.ReadAsBytes(), Path.GetFileName(((FileStream)stream).Name));
      }
      else
      {
        return FileParameter.Create(name, stream.ReadAsBytes(), "no_file_name_provided");
      }
    }

    /// <summary>
    /// If parameter is DateTime, output in a formatted string (default ISO 8601), customizable with Configuration.DateTime.
    /// If parameter is a list of string, join the list with ",".
    /// Otherwise just return the string.
    /// </summary>
    /// <param name="obj">The parameter (header, path, query, form).</param>
    /// <returns>Formatted string.</returns>
    protected string ParameterToString(object obj)
    {
      if (obj == null)
      {
        return null;
      }

      if (obj is DateTime)
      {
        // Return a formatted date string - Can be customized with Configuration.DateTimeFormat
        // Defaults to an ISO 8601, using the known as a Round-trip date/time pattern ("o")
        // https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx#Anchor_8
        // For example: 2009-06-15T13:45:30.0000000
        return ((DateTime)obj).ToString(Configuration.DateTimeFormat);
      }
      else if (obj is List<string>)
      {
        return string.Join(",", (obj as List<string>).ToArray());
      }
      else
      {
        return Convert.ToString(obj);
      }
    }

    /// <summary>
    /// Deserialize the JSON string into a proper object.
    /// </summary>
    /// <param name="content">HTTP body (e.g. string, JSON).</param>
    /// <param name="type">Object type.</param>
    /// <param name="headers">HTTP headers.</param>
    /// <returns>Object representation of the JSON string.</returns>
    protected object Deserialize(string content, Type type, IList<Parameter> headers = null)
    {
      // return an object
      if (type == typeof(object))
      {
        return content;
      }

      if (type == typeof(Stream))
      {
        var filePath = string.IsNullOrEmpty(Configuration.TempFolderPath)
            ? Path.GetTempPath()
            : Configuration.TempFolderPath;

        var fileName = filePath + Guid.NewGuid();
        if (headers != null)
        {
          var regex = new Regex(@"Content-Disposition:.*filename=['""]?([^'""\s]+)['""]?$");
          var match = regex.Match(headers.ToString());
          if (match.Success)
          {
            fileName = filePath + match.Value.Replace("\"", string.Empty).Replace("'", string.Empty);
          }
        }

        File.WriteAllText(fileName, content);
        return new FileStream(fileName, FileMode.Open);
      }

      // return a datetime object
      if (type.Name.StartsWith("System.Nullable`1[[System.DateTime"))
      {
        return DateTime.Parse(content, null, System.Globalization.DateTimeStyles.RoundtripKind);
      }

      // return primitive type
      if (type == typeof(string) || type.Name.StartsWith("System.Nullable"))
      {
        return ConvertType(content, type);
      }

      return JsonConvert.DeserializeObject(content, type);
    }

    /// <summary>
    /// Serialize an object into JSON string.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <returns>JSON string.</returns>
    protected string Serialize(object obj)
    {
      return obj != null ? JsonConvert.SerializeObject(obj) : null;
    }

    /// <summary>
    /// Get the API key with prefix.
    /// </summary>
    /// <param name="apiKeyIdentifier">API key identifier (authentication scheme).</param>
    /// <returns>API key with prefix.</returns>
    protected string GetApiKeyWithPrefix(string apiKeyIdentifier)
    {
      var apiKeyValue = string.Empty;
      Configuration.ApiKey.TryGetValue(apiKeyIdentifier, out apiKeyValue);
      var apiKeyPrefix = string.Empty;
      if (Configuration.ApiKeyPrefix.TryGetValue(apiKeyIdentifier, out apiKeyPrefix))
      {
        return apiKeyPrefix + " " + apiKeyValue;
      }
      else
      {
        return apiKeyValue;
      }
    }

    /// <summary>
    /// Update parameters based on authentication.
    /// </summary>
    /// <param name="headerParams">Header parameters.</param>
    protected void UpdateParamsForAuth(Dictionary<string, string> headerParams)
    {
      switch (Configuration.AuthenticationType)
      {
        case AuthenticationType.Anonymous:
          break;
        case AuthenticationType.BasicAuthentication:
          RestClient.Authenticator = new HttpBasicAuthenticator(Configuration.Username, Configuration.Password);
          break;
        case AuthenticationType.ApiKey:
          headerParams["api_key"] = GetApiKeyWithPrefix("api_key");
          break;
        case AuthenticationType.OAuth2:
          RestClient.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(Configuration.AccessToken);
          break;
      }
    }

    /// <summary>
    /// Encode string in base64 format.
    /// </summary>
    /// <param name="text">String to be encoded.</param>
    /// <returns>Encoded string.</returns>
    protected string Base64Encode(string text)
    {
      var textByte = System.Text.Encoding.UTF8.GetBytes(text);
      return System.Convert.ToBase64String(textByte);
    }

    /// <summary>
    /// Dynamically cast the object into target type.
    /// Ref: http://stackoverflow.com/questions/4925718/c-dynamic-runtime-cast
    /// </summary>
    /// <param name="source">Object to be casted</param>
    /// <param name="dest">Target type</param>
    /// <returns>Casted object</returns>
    protected object ConvertType(object source, Type dest)
    {
      return Convert.ChangeType(source, dest);
    }

    /// <summary>
    /// Convert first character in a string to lower
    /// </summary>
    /// <param name="str">String</param>
    /// <returns>Modified string</returns>
    protected string FirstCharacterToLower(string str)
    {
      if (string.IsNullOrEmpty(str) || char.IsLower(str, 0))
      {
        return str;
      }

      return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }

    /// <summary>
    /// Deserialize and populate response content
    /// </summary>
    /// <param name="content">Response content</param>
    /// <param name="entity">Entity reference</param>
    /// <param name="headers">List of headers</param>
    protected void DeserializeAndPopulate(string content, ref TEntity entity, IList<Parameter> headers)
    {
      if (EntityType == typeof(object)
        || EntityType == typeof(Stream)
        || EntityType.Name.StartsWith("System.Nullable`1[[System.DateTime")
        || EntityType == typeof(string)
        || EntityType.Name.StartsWith("System.Nullable"))
      {
        entity = (TEntity)Deserialize(content, typeof(TEntity), headers);
        return;
      }

      if (!string.IsNullOrWhiteSpace(content))
      {
        JsonConvert.PopulateObject(content, entity);
      }
    }

    /// <summary>
    /// Deserialize and populate response content
    /// </summary>
    /// <param name="content">Response content</param>
    /// <param name="entities">List of entities</param>
    /// <param name="headers">List of headers</param>
    protected void DeserializeAndPopulate(string content, ref IEnumerable<TEntity> entities, IList<Parameter> headers)
    {
      if (EntityType == typeof(object)
        || EntityType == typeof(Stream)
        || EntityType.Name.StartsWith("System.Nullable`1[[System.DateTime")
        || EntityType == typeof(string)
        || EntityType.Name.StartsWith("System.Nullable"))
      {
        entities = (IEnumerable<TEntity>)Deserialize(content, typeof(IEnumerable<TEntity>), headers);
        return;
      }

      if (!string.IsNullOrWhiteSpace(content))
      {
        JsonConvert.PopulateObject(content, entities);
      }
    }

    /// <summary>
    /// Get the name of a property from an expression
    /// </summary>
    /// <param name="propertyExpression">Property expression</param>
    /// <returns>Property name</returns>
    protected string GetName(Expression<Func<TEntity, object>> propertyExpression)
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
    /// Get a list of parameter names from an entity path. For example { "Id" } from /posts/{id}
    /// </summary>
    /// <param name="path">Entity path</param>
    /// <returns>List of property names</returns>
    protected List<string> GetParameterNamesFromPath(string path)
    {
      var matches = Regex.Matches(path, @"\{\w*\}");
      var result = new List<string>();
      foreach (var m in matches)
      {
        result.Add(m.ToString()
          .Replace("{", string.Empty)
          .Replace("}", string.Empty));
      }

      return result;
    }

    /// <summary>
    /// Makes the HTTP request (Sync).
    /// </summary>
    /// <param name="path">URL path.</param>
    /// <param name="method">HTTP method.</param>
    /// <param name="pathParams">Path parameters</param>
    /// <param name="queryParams">Query parameters.</param>
    /// <param name="postBody">HTTP body (POST request).</param>
    /// <param name="headerParams">Header parameters.</param>
    /// <param name="formParams">Form parameters.</param>
    /// <param name="fileParams">File parameters.</param>
    /// <returns>Object</returns>
    public Task<IRestResponse> CallApiAsync(string path,
      RestSharp.Method method,
      Dictionary<string, string> pathParams,
      Dictionary<string, string> queryParams,
      string postBody,
      Dictionary<string, string> headerParams,
      Dictionary<string, string> formParams,
      Dictionary<string, FileParameter> fileParams)
    {
      var request = new RestRequest(path, method);

      UpdateParamsForAuth(headerParams);

      // add default header, if any
      foreach (var defaultHeader in DefaultHeaderMap)
      {
        request.AddHeader(defaultHeader.Key, defaultHeader.Value);
      }

      // add header parameter, if any
      foreach (var param in headerParams)
      {
        request.AddHeader(param.Key, param.Value);
      }

      // add path parameter, if any
      foreach (var param in pathParams)
      {
        request.AddParameter(param.Key, param.Value, ParameterType.UrlSegment);
      }

      // add query parameter, if any
      foreach (var param in queryParams)
      {
        request.AddParameter(param.Key, param.Value, ParameterType.GetOrPost);
      }

      // add form parameter, if any
      foreach (var param in formParams)
      {
        request.AddParameter(param.Key, param.Value, ParameterType.GetOrPost);
      }

      // add file parameter, if any
      foreach (var param in fileParams)
      {
#if NET452
        request.AddFile(
        param.Value.Name,
        param.Value.Writer,
        param.Value.FileName,
        param.Value.ContentType);
#else
        request.AddFile(
          param.Value.Name,
          param.Value.Writer,
          param.Value.FileName,
          param.Value.ContentLength,
          param.Value.ContentType);
#endif
      }

      // http body (model) parameter
      if (postBody != null)
      {
        request.AddParameter("application/json", postBody, ParameterType.RequestBody);
      }
      var taskCompletionSource = new TaskCompletionSource<IRestResponse>();
      RestClient.ExecuteAsync(request, (response) => taskCompletionSource.SetResult(response));
      return taskCompletionSource.Task;
    }

    /// <summary>
    /// Get Id property value of en entity instance
    /// </summary>
    /// <param name="entity">Entity instance</param>
    /// <returns>Id property value</returns>
    protected object GetIdPropertyValue(TEntity entity)
    {
      if (string.IsNullOrEmpty(IdPropertyName))
      {
        return null;
      }

      return EntityType.GetProperty(IdPropertyName)?.GetValue(entity, null);
    }
  }
}
