using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryFramework.Api
{
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
    OAuth2
  }
}
