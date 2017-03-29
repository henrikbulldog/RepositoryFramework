using System;

namespace RepositoryFramework.Api
{
  /// <summary>
  /// API Exception
  /// </summary>
  public class ApiException : Exception
	{
		/// <summary>
		/// Gets or sets the error code (HTTP status code)
		/// </summary>
		/// <value>The error code (HTTP status code).</value>
		public int ErrorCode { get; private set; }

		/// <summary>
		/// Gets or sets the error content (body json object)
		/// </summary>
		/// <value>The error content (Http response body).</value>
		public Object ErrorContent { get; private set; }

    /// <summary>
    /// HTTP Method
    /// </summary>
    public string Method { get; private set; }

    /// <summary>
    /// Base path
    /// </summary>
    public string BasePath { get; private set; }

    /// <summary>
    /// Request path
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Request parameters
    /// </summary>
    public object Filter { get; set; }

    /// <summary>
    /// Entity used in request
    /// </summary>
    public object Entity { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class.
    /// </summary>
    public ApiException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class.
    /// </summary>
    public ApiException(int errorCode, string message, string method, string basePath, string path, 
      object filter = null, object entity = null, object errorContent = null) : base(message)
    {
      ErrorCode = errorCode;
      Method = method;
      BasePath = basePath;
      Path = path;
      Filter = filter;
      Entity = entity;
      ErrorContent = errorContent;
    }

  }

}
