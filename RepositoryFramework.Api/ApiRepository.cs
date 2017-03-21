using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Extensions;
using System.Threading;
using System.Reflection;
using System.Linq;
using RepositoryFramework.Interfaces;
using RestSharp.Authenticators;
using System.Linq.Expressions;

namespace RepositoryFramework.Api
{
  public class ApiRepository<TEntity>
    : IRepository<TEntity>
    , IGet<TEntity, string>
    , IFind<TEntity>
    where TEntity : class
  {
    protected Configuration configuration = null;

    protected readonly Dictionary<String, String> _defaultHeaderMap = new Dictionary<String, String>();

    protected Type EntityType { get; set; }

    protected string EntityTypeName { get; set; }

    protected string EntityPath { get; set; }

    protected string EntityIdPropertyName { get; set; }

    protected List<string> PathParameters { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiClient" /> class.
    /// </summary>
    /// <param name="basePath">The base path.</param>
    /// <param name="entityPath">Path excluding the base path. May contain path paramater placeholders in the format {someParm}</param>
    public ApiRepository(Configuration configuration, string basePath, string entityPath = null, 
      Expression<Func<TEntity, object>> entityIdProperty = null)
    {
      this.configuration = configuration;
      BasePath = basePath;
      RestClient = new RestClient(BasePath);

      EntityType = typeof(TEntity);
      EntityTypeName = EntityType.Name;
      if (string.IsNullOrEmpty(entityPath))
      {
        EntityPath = $"/{FirstCharacterToLower(EntityTypeName)}s";
      }
      else
      {
        EntityPath = entityPath;
      }
      if (entityIdProperty != null)
      {
        EntityIdPropertyName = GetName(entityIdProperty);
      }
      else
      {
        EntityIdPropertyName = FindIdProperty(typeof(TEntity));
      }
      PathParameters = GetParametersFromPath(EntityPath);
    }

    protected static string GetName(Expression<Func<TEntity, object>> exp)
    {
      var body = exp.Body as MemberExpression;

      if (body != null) return body.Member.Name;

      var ubody = (UnaryExpression)exp.Body;
      body = ubody.Operand as MemberExpression;

      return body?.Member.Name ?? string.Empty;
    }

    private List<string> GetParametersFromPath(string entityPath)
    {
      var matches = Regex.Matches(entityPath, @"\{\w*\}");
      var result = new List<string>();
      foreach (var m in matches)
      {
        result.Add(m.ToString()
          .Replace("{", "")
          .Replace("}", ""));
      }
      return result;
    }

    protected virtual string FindIdProperty(Type type)
    {
      var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
      var idProperty = properties
        .FirstOrDefault(p => p.Name.ToLower() == $"{EntityTypeName.ToLower()}id");
      if (idProperty == null)
      {
        idProperty = properties
          .FirstOrDefault(p => p.Name.ToLower() == "id");
      }
      if (idProperty == null)
      {
        throw new Exception($"Cannot determine id property of type {type.Name}");
      }
      return idProperty.Name;
    }

    public virtual void Create(TEntity entity)
    {
      if (entity == null)
      {
        throw new ApiException(400, $"Missing required parameter entity when calling APIRepository<{EntityTypeName}>.Update()");
      }

      var path = EntityPath;
      path = path.Replace("{format}", "json");

      var pathParams = new Dictionary<String, String>();
      var queryParams = new Dictionary<String, String>();
      var headerParams = new Dictionary<String, String>();
      var formParams = new Dictionary<String, String>();
      var fileParams = new Dictionary<String, FileParameter>();
      string postBody = Serialize(entity);

      foreach (string property in entity
        .GetType()
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Select(p => p.Name))
      {
        var parameterValue = ParameterToString(entity.GetType().GetProperty(property).GetValue(entity));
        var pathParmeter = PathParameters.FirstOrDefault(p => p.ToLower() == property.ToLower());
        if (pathParmeter != null)
        {
          if (parameterValue == null)
          {
            throw new Exception($"Path parameter {pathParmeter} cannot be null");
          }
          pathParams.Add(pathParmeter, parameterValue);
        }
      }

      IRestResponse response = (IRestResponse)CallApi(
          path, Method.POST, pathParams, queryParams, postBody, headerParams, formParams, fileParams);

      if (((int)response.StatusCode) >= 400)
        throw new ApiException((int)response.StatusCode,
          $"Error calling APIRepository<{EntityTypeName}>.Update: {response.Content}", response.Content);
      else if (((int)response.StatusCode) == 0)
        throw new ApiException((int)response.StatusCode,
          $"Error calling APIRepository<{EntityTypeName}>.Update: {response.ErrorMessage}", response.ErrorMessage);

      DeserializeAndPopulate(response.Content, ref entity, response.Headers);
    }

    private void DeserializeAndPopulate(string content, ref TEntity entity, IList<Parameter> headers)
    {
      if (EntityType == typeof(Object)
        || EntityType == typeof(Stream)
        || EntityType.Name.StartsWith("System.Nullable`1[[System.DateTime")
        || EntityType == typeof(String) 
        || EntityType.Name.StartsWith("System.Nullable")) 
      {
        entity = (TEntity)Deserialize(content, typeof(TEntity), headers);
        return;
      }

      try
      {
        JsonConvert.PopulateObject(content, entity);
      }
      catch (IOException e)
      {
        throw new ApiException(500, e.Message);
      }
    }

    public virtual void Delete(TEntity entity)
    {
      if (entity == null)
      {
        throw new ApiException(400, $"Missing required parameter entity when calling APIRepository<{EntityTypeName}>.Update()");
      }

      var path = $"{EntityPath}/{GetIdPropertyValue(entity)}";
      path = path.Replace("{format}", "json");

      var pathParams = new Dictionary<String, String>();
      var queryParams = new Dictionary<String, String>();
      var headerParams = new Dictionary<String, String>();
      var formParams = new Dictionary<String, String>();
      var fileParams = new Dictionary<String, FileParameter>();
      string postBody = Serialize(entity);

      foreach (string property in entity
        .GetType()
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Select(p => p.Name))
      {
        var parameterValue = ParameterToString(entity.GetType().GetProperty(property).GetValue(entity));
        var pathParmeter = PathParameters.FirstOrDefault(p => p.ToLower() == property.ToLower());
        if (pathParmeter != null)
        {
          if (parameterValue == null)
          {
            throw new Exception($"Path parameter {pathParmeter} cannot be null");
          }
          pathParams.Add(pathParmeter, parameterValue);
        }
      }

      IRestResponse response = (IRestResponse)CallApi(
      path, Method.DELETE, pathParams, queryParams, postBody, headerParams, formParams, fileParams);

      if (((int)response.StatusCode) >= 400)
        throw new ApiException((int)response.StatusCode,
          $"Error calling APIRepository<{EntityTypeName}>.Delete: {response.Content}", response.Content);
      else if (((int)response.StatusCode) == 0)
        throw new ApiException((int)response.StatusCode,
          $"Error calling APIRepository<{EntityTypeName}>.Delete: {response.ErrorMessage}", response.ErrorMessage);
    }

    public virtual TEntity GetById(string filter)
    {
      if (filter == null)
      {
        throw new ApiException(400, $"Missing required parameter filter when calling APIRepository<{EntityTypeName}>.GetById()");
      }
      var path = $"{EntityPath}/{filter}";
      path = path.Replace("{format}", "json");

      var pathParams = new Dictionary<String, String>();
      var queryParams = new Dictionary<String, String>();
      var headerParams = new Dictionary<String, String>();
      var formParams = new Dictionary<String, String>();
      var fileParams = new Dictionary<String, FileParameter>();
      String postBody = null;

      IRestResponse response = (IRestResponse)CallApi(
        path, Method.GET, pathParams, queryParams, postBody, headerParams, formParams, fileParams);

      if (((int)response.StatusCode) >= 400)
        throw new ApiException((int)response.StatusCode,
          $"Error calling APIRepository<{EntityTypeName}>.GetById: {response.Content}", response.Content);
      else if (((int)response.StatusCode) == 0)
        throw new ApiException((int)response.StatusCode,
          $"Error calling APIRepository<{EntityTypeName}>.GetById: {response.ErrorMessage}", response.ErrorMessage);

      return (TEntity)Deserialize(response.Content, typeof(TEntity), response.Headers);
    }

    public virtual void Update(TEntity entity)
    {
      if (entity == null)
      {
        throw new ApiException(400, $"Missing required parameter entity when calling APIRepository<{EntityTypeName}>.Update()");
      }

      var path = $"{EntityPath}/{GetIdPropertyValue(entity)}";
      path = path.Replace("{format}", "json");

      var pathParams = new Dictionary<String, String>();
      var queryParams = new Dictionary<String, String>();
      var headerParams = new Dictionary<String, String>();
      var formParams = new Dictionary<String, String>();
      var fileParams = new Dictionary<String, FileParameter>();
      string postBody = Serialize(entity);

      foreach (string property in entity
        .GetType()
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Select(p => p.Name))
      {
        var parameterValue = ParameterToString(entity.GetType().GetProperty(property).GetValue(entity));
        var pathParmeter = PathParameters.FirstOrDefault(p => p.ToLower() == property.ToLower());
        if (pathParmeter != null)
        {
          if (parameterValue == null)
          {
            throw new Exception($"Path parameter {pathParmeter} cannot be null");
          }
          pathParams.Add(pathParmeter, parameterValue);
        }
      }

      IRestResponse response = (IRestResponse)CallApi(
        path, Method.PUT, pathParams, queryParams, postBody, headerParams, formParams, fileParams);

      if (((int)response.StatusCode) >= 400)
        throw new ApiException((int)response.StatusCode,
          $"Error calling APIRepository<{EntityTypeName}>.Update: {response.Content}", response.Content);
      else if (((int)response.StatusCode) == 0)
        throw new ApiException((int)response.StatusCode,
          $"Error calling APIRepository<{EntityTypeName}>.Update: {response.ErrorMessage}", response.ErrorMessage);

      DeserializeAndPopulate(response.Content, ref entity, response.Headers);
    }

    private object GetIdPropertyValue(TEntity entity)
    {
      return entity.GetType().GetProperty(EntityIdPropertyName).GetValue(entity, null);
    }

    /// <summary>
    /// Gets or sets the base path.
    /// </summary>
    /// <value>The base path</value>
    public string BasePath { get; set; }

    /// <summary>
    /// Gets or sets the RestClient.
    /// </summary>
    /// <value>An instance of the RestClient</value>
    public RestClient RestClient { get; set; }

    /// <summary>
    /// Gets the default header.
    /// </summary>
    public Dictionary<String, String> DefaultHeader
    {
      get { return _defaultHeaderMap; }
    }

    /// <summary>
    /// Makes the HTTP request (Sync).
    /// </summary>
    /// <param name="path">URL path.</param>
    /// <param name="method">HTTP method.</param>
    /// <param name="queryParams">Query parameters.</param>
    /// <param name="postBody">HTTP body (POST request).</param>
    /// <param name="headerParams">Header parameters.</param>
    /// <param name="formParams">Form parameters.</param>
    /// <param name="fileParams">File parameters.</param>
    /// <param name="authSettings">Authentication settings.</param>
    /// <returns>Object</returns>
    public Object CallApi(String path, RestSharp.Method method, Dictionary<String, String> pathParams,
      Dictionary<String, String> queryParams, String postBody,
      Dictionary<String, String> headerParams, Dictionary<String, String> formParams,
      Dictionary<String, FileParameter> fileParams)
    {

      var request = new RestRequest(path, method);

      UpdateParamsForAuth(headerParams);

      // add default header, if any
      foreach (var defaultHeader in _defaultHeaderMap)
        request.AddHeader(defaultHeader.Key, defaultHeader.Value);

      // add header parameter, if any
      foreach (var param in headerParams)
        request.AddHeader(param.Key, param.Value);

      // add path parameter, if any
      foreach (var param in pathParams)
        request.AddParameter(param.Key, param.Value, ParameterType.UrlSegment);

      // add query parameter, if any
      foreach (var param in queryParams)
        request.AddParameter(param.Key, param.Value, ParameterType.GetOrPost);

      // add form parameter, if any
      foreach (var param in formParams)
        request.AddParameter(param.Key, param.Value, ParameterType.GetOrPost);

      // add file parameter, if any
      foreach (var param in fileParams)
      {
#if NET452
        request.AddFile(param.Value.Name, param.Value.Writer, param.Value.FileName,
          param.Value.ContentType);
#else
        request.AddFile(param.Value.Name, param.Value.Writer, param.Value.FileName,
          param.Value.ContentLength, param.Value.ContentType);
#endif
      }

      if (postBody != null) // http body (model) parameter
        request.AddParameter("application/json", postBody, ParameterType.RequestBody);

      EventWaitHandle handle = new AutoResetEvent(false);
      Object result = null;
      RestClient.ExecuteAsync<Object>(request, r =>
      {
        result = r;
        handle.Set();
      });
      handle.WaitOne();

      return result;
    }

    /// <summary>
    /// Add default header.
    /// </summary>
    /// <param name="key">Header field name.</param>
    /// <param name="value">Header field value.</param>
    /// <returns></returns>
    public void AddDefaultHeader(string key, string value)
    {
      _defaultHeaderMap.Add(key, value);
    }

    /// <summary>
    /// Escape string (url-encoded).
    /// </summary>
    /// <param name="str">String to be escaped.</param>
    /// <returns>Escaped string.</returns>
    public string EscapeString(string str)
    {
      return RestSharp.Extensions.StringExtensions.UrlEncode(str);
    }

    /// <summary>
    /// Create FileParameter based on Stream.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="stream">Input stream.</param>
    /// <returns>FileParameter.</returns>
    public FileParameter ParameterToFile(string name, Stream stream)
    {
      if (stream is FileStream)
        return FileParameter.Create(name, stream.ReadAsBytes(), Path.GetFileName(((FileStream)stream).Name));
      else
        return FileParameter.Create(name, stream.ReadAsBytes(), "no_file_name_provided");
    }

    /// <summary>
    /// If parameter is DateTime, output in a formatted string (default ISO 8601), customizable with Configuration.DateTime.
    /// If parameter is a list of string, join the list with ",".
    /// Otherwise just return the string.
    /// </summary>
    /// <param name="obj">The parameter (header, path, query, form).</param>
    /// <returns>Formatted string.</returns>
    public string ParameterToString(object obj)
    {
      if(obj == null)
      {
        return null;
      }
      if (obj is DateTime)
        // Return a formatted date string - Can be customized with Configuration.DateTimeFormat
        // Defaults to an ISO 8601, using the known as a Round-trip date/time pattern ("o")
        // https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx#Anchor_8
        // For example: 2009-06-15T13:45:30.0000000
        return ((DateTime)obj).ToString(configuration.DateTimeFormat);
      else if (obj is List<string>)
        return String.Join(",", (obj as List<string>).ToArray());
      else
        return Convert.ToString(obj);
    }

    /// <summary>
    /// Deserialize the JSON string into a proper object.
    /// </summary>
    /// <param name="content">HTTP body (e.g. string, JSON).</param>
    /// <param name="type">Object type.</param>
    /// <param name="headers">HTTP headers.</param>
    /// <returns>Object representation of the JSON string.</returns>
    public object Deserialize(string content, Type type, IList<Parameter> headers = null)
    {
      if (type == typeof(Object)) // return an object
      {
        return content;
      }

      if (type == typeof(Stream))
      {
        var filePath = String.IsNullOrEmpty(Configuration.TempFolderPath)
            ? Path.GetTempPath()
            : Configuration.TempFolderPath;

        var fileName = filePath + Guid.NewGuid();
        if (headers != null)
        {
          var regex = new Regex(@"Content-Disposition:.*filename=['""]?([^'""\s]+)['""]?$");
          var match = regex.Match(headers.ToString());
          if (match.Success)
            fileName = filePath + match.Value.Replace("\"", "").Replace("'", "");
        }
        File.WriteAllText(fileName, content);
        return new FileStream(fileName, FileMode.Open);

      }

      if (type.Name.StartsWith("System.Nullable`1[[System.DateTime")) // return a datetime object
      {
        return DateTime.Parse(content, null, System.Globalization.DateTimeStyles.RoundtripKind);
      }

      if (type == typeof(String) || type.Name.StartsWith("System.Nullable")) // return primitive type
      {
        return ConvertType(content, type);
      }

      // at this point, it must be a model (json)
      try
      {
        return JsonConvert.DeserializeObject(content, type);
      }
      catch (IOException e)
      {
        throw new ApiException(500, e.Message);
      }
    }

    /// <summary>
    /// Serialize an object into JSON string.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <returns>JSON string.</returns>
    public string Serialize(object obj)
    {
      try
      {
        return obj != null ? JsonConvert.SerializeObject(obj) : null;
      }
      catch (Exception e)
      {
        throw new ApiException(500, e.Message);
      }
    }

    /// <summary>
    /// Get the API key with prefix.
    /// </summary>
    /// <param name="apiKeyIdentifier">API key identifier (authentication scheme).</param>
    /// <returns>API key with prefix.</returns>
    public string GetApiKeyWithPrefix(string apiKeyIdentifier)
    {
      var apiKeyValue = "";
      configuration.ApiKey.TryGetValue(apiKeyIdentifier, out apiKeyValue);
      var apiKeyPrefix = "";
      if (configuration.ApiKeyPrefix.TryGetValue(apiKeyIdentifier, out apiKeyPrefix))
        return apiKeyPrefix + " " + apiKeyValue;
      else
        return apiKeyValue;
    }

    /// <summary>
    /// Update parameters based on authentication.
    /// </summary>
    /// <param name="queryParams">Query parameters.</param>
    /// <param name="headerParams">Header parameters.</param>
    public void UpdateParamsForAuth(Dictionary<String, String> headerParams)
    {
      switch (configuration.AuthenticationType)
      {
        case AuthenticationType.Anonymous:
          break;
        case AuthenticationType.BasicAuthentication:
          RestClient.Authenticator = new HttpBasicAuthenticator(configuration.Username, configuration.Password);
          //headerParams["Authorization"] = "Basic " + Base64Encode(configuration.Username + ":" + configuration.Password);
          break;
        case AuthenticationType.ApiKey:
          headerParams["api_key"] = GetApiKeyWithPrefix("api_key");
          break;
        case AuthenticationType.OAuth2:
          RestClient.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator("");
          //TODO support oauth
          break;
      }
    }

    /// <summary>
    /// Encode string in base64 format.
    /// </summary>
    /// <param name="text">String to be encoded.</param>
    /// <returns>Encoded string.</returns>
    public static string Base64Encode(string text)
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
    public static Object ConvertType(Object source, Type dest)
    {
      return Convert.ChangeType(source, dest);
    }

    public static string FirstCharacterToLower(string str)
    {
      if (String.IsNullOrEmpty(str) || Char.IsLower(str, 0))
        return str;

      return Char.ToLowerInvariant(str[0]) + str.Substring(1);
    }

    public virtual IQueryResult<TEntity> Find()
    {
      var path = EntityPath;
      path = path.Replace("{format}", "json");

      var pathParams = new Dictionary<String, String>();
      var queryParams = new Dictionary<String, String>();
      var headerParams = new Dictionary<String, String>();
      var formParams = new Dictionary<String, String>();
      var fileParams = new Dictionary<String, FileParameter>();
      String postBody = null;

      IRestResponse response = (IRestResponse)CallApi(
        path, Method.GET, pathParams, queryParams, postBody, headerParams, formParams, fileParams);

      if (((int)response.StatusCode) >= 400)
        throw new ApiException((int)response.StatusCode,
          $"Error calling {EntityType}Repository.Find(): {response.Content}", response.Content);
      else if (((int)response.StatusCode) == 0)
        throw new ApiException((int)response.StatusCode,
          $"Error calling {EntityType}Repository.Find(): {response.ErrorMessage}", response.ErrorMessage);

      var result = (List<TEntity>)Deserialize(response.Content, typeof(List<TEntity>), response.Headers);
      return new QueryResult<TEntity>(result, result.Count);
    }
  }
}