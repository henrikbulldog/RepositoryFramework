using System;
using System.Collections.Generic;
using System.IO;

namespace RepositoryFramework.Api
{
  /// <summary>
  /// Represents a set of configuration settings
  /// </summary>
  public class Configuration
  {
    private const string Iso8601DateTimeFormat = "o";

    private static string tempFolderPath = Path.GetTempPath();

    private string dateTimeFormat = Iso8601DateTimeFormat;

    /// <summary>
    /// Gets or sets the temporary folder path to store the files downloaded from the server.
    /// </summary>
    /// <value>Folder path.</value>
    public static string TempFolderPath
    {
      get
      {
        return tempFolderPath;
      }

      set
      {
        if (string.IsNullOrEmpty(value))
        {
          tempFolderPath = value;
          return;
        }

        // create the directory if it does not exist
        if (!Directory.Exists(value))
        {
          Directory.CreateDirectory(value);
        }

        // check if the path contains directory separator at the end
        if (value[value.Length - 1] == Path.DirectorySeparatorChar)
        {
          tempFolderPath = value;
        }
        else
        {
          tempFolderPath = value + Path.DirectorySeparatorChar;
        }
      }
    }

    /// <summary>
    /// Gets or sets the username (HTTP basic authentication).
    /// </summary>
    /// <value>The username.</value>
    public string Username { get; set; }

    /// <summary>
    /// Gets or sets the password (HTTP basic authentication).
    /// </summary>
    /// <value>The password.</value>
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets authentication settings
    /// </summary>
    public AuthenticationType AuthenticationType { get; set; } = AuthenticationType.Anonymous;

    /// <summary>
    /// Gets or sets the API key based on the authentication name.
    /// </summary>
    /// <value>The API key.</value>
    public Dictionary<string, string> ApiKey { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the prefix (e.g. Token) of the API key based on the authentication name.
    /// </summary>
    /// <value>The prefix of the API key.</value>
    public Dictionary<string, string> ApiKeyPrefix { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the the date time format used when serializing in the ApiClient
    /// By default, it's set to ISO 8601 - "o", for others see:
    /// https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx
    /// and https://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx
    /// No validation is done to ensure that the string you're providing is valid
    /// </summary>
    /// <value>The DateTimeFormat string</value>
    public string DateTimeFormat
    {
      get
      {
        return dateTimeFormat;
      }

      set
      {
        if (string.IsNullOrEmpty(value))
        {
          // Never allow a blank or null string, go back to the default
          dateTimeFormat = Iso8601DateTimeFormat;
          return;
        }

        // Caution, no validation when you choose date time format other than ISO 8601
        // Take a look at the above links
        dateTimeFormat = value;
      }
    }

    /// <summary>
    /// Gets or sets OAuth2 access token
    /// </summary>
    public string AccessToken { get; set; }
  }
}
