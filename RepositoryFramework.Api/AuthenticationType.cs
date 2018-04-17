namespace RepositoryFramework.Api
{
  /// <summary>
  /// Authentication type
  /// </summary>
  public enum AuthenticationType
  {
    /// <summary>
    /// Anumynous access, no authentication
    /// </summary>
    Anonymous,

    /// <summary>
    /// API key authentication
    /// </summary>
    ApiKey,

    /// <summary>
    /// Basic authentication. HTTP Authorization header containing [user name]:[password]
    /// </summary>
    BasicAuthentication,

    /// <summary>
    /// OAuth2 / OpenID Connect token authentication
    /// </summary>
    OAuth2,

    /// <summary>
    /// Jwt authentication
    /// </summary>
    Jwt
  }
}
