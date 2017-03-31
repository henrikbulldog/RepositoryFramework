using System;

namespace RepositoryFramework.Api
{
  /// <summary>
  /// API Exception
  /// </summary>
  public class ApiException : Exception
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class.
    /// </summary>
    public ApiException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class.
    /// </summary>
    /// <param name="errorCode">Error code</param>
    /// <param name="message">Message</param>
    /// <param name="method">Method</param>
    /// <param name="basePath">Base path</param>
    /// <param name="path">Entity path</param>
    /// <param name="filter">request filter</param>
    /// <param name="entity">Entity</param>
    /// <param name="errorContent">Error content</param>
    public ApiException(
      int errorCode,
      string message,
      string method,
      string basePath,
      string path,
      object filter = null,
      object entity = null,
      object errorContent = null)
      : base(message)
    {
      ErrorCode = errorCode;
      Method = method;
      BasePath = basePath;
      Path = path;
      Filter = filter;
      Entity = entity;
      ErrorContent = errorContent;
    }

    /// <summary>
    /// Gets the error code (HTTP status code)
    /// </summary>
    /// <value>The error code (HTTP status code).</value>
    public int ErrorCode { get; private set; }

    /// <summary>
    /// Gets the error content (body json object)
    /// </summary>
    /// <value>The error content (Http response body).</value>
    public object ErrorContent { get; private set; }

    /// <summary>
    /// Gets HTTP Method
    /// </summary>
    public string Method { get; private set; }

    /// <summary>
    /// Gets Base path
    /// </summary>
    public string BasePath { get; private set; }

    /// <summary>
    /// Gets Request path
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Gets request parameters filter object
    /// </summary>
    public object Filter { get; private set; }

    /// <summary>
    /// Gets Entity used in request
    /// </summary>
    public object Entity { get; private set; }
  }
}
