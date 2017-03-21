using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RepositoryFramework.Api
{
	/// <summary>
	/// Represents a set of configuration settings
	/// </summary>
	public class Configuration
	{

		/// <summary>
		/// Gets or sets the username (HTTP basic authentication).
		/// </summary>
		/// <value>The username.</value>
		public String Username { get; set; }

		/// <summary>
		/// Gets or sets the password (HTTP basic authentication).
		/// </summary>
		/// <value>The password.</value>
		public String Password { get; set; }

    /// <summary>
    /// Authentication settings
    /// </summary>
    public AuthenticationType AuthenticationType { get; set; } = AuthenticationType.Anonymous;

    /// <summary>
    /// Gets or sets the API key based on the authentication name.
    /// </summary>
    /// <value>The API key.</value>
    public Dictionary<String, String> ApiKey = new Dictionary<String, String>();

		/// <summary>
		/// Gets or sets the prefix (e.g. Token) of the API key based on the authentication name.
		/// </summary>
		/// <value>The prefix of the API key.</value>
		public Dictionary<String, String> ApiKeyPrefix = new Dictionary<String, String>();

		private static string _tempFolderPath = Path.GetTempPath();

		/// <summary>
		/// Gets or sets the temporary folder path to store the files downloaded from the server.
		/// </summary>
		/// <value>Folder path.</value>
		public static String TempFolderPath
		{
			get { return _tempFolderPath; }

			set
			{
				if (String.IsNullOrEmpty(value))
				{
					_tempFolderPath = value;
					return;
				}

				// create the directory if it does not exist
				if (!Directory.Exists(value))
					Directory.CreateDirectory(value);

				// check if the path contains directory separator at the end
				if (value[value.Length - 1] == Path.DirectorySeparatorChar)
					_tempFolderPath = value;
				else
					_tempFolderPath = value + Path.DirectorySeparatorChar;
			}
		}

		private const string ISO8601_DATETIME_FORMAT = "o";

		private string _dateTimeFormat = ISO8601_DATETIME_FORMAT;

		/// <summary>
		/// Gets or sets the the date time format used when serializing in the ApiClient
		/// By default, it's set to ISO 8601 - "o", for others see:
		/// https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx
		/// and https://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx
		/// No validation is done to ensure that the string you're providing is valid
		/// </summary>
		/// <value>The DateTimeFormat string</value>
		public String DateTimeFormat
		{
			get
			{
				return _dateTimeFormat;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					// Never allow a blank or null string, go back to the default
					_dateTimeFormat = ISO8601_DATETIME_FORMAT;
					return;
				}

				// Caution, no validation when you choose date time format other than ISO 8601
				// Take a look at the above links
				_dateTimeFormat = value;
			}
		}

		/// <summary>
		/// Returns a string with essential information for debugging.
		/// </summary>
		public String ToDebugReport()
		{
			String report = "C# SDK (FossAssureMosaicAPIClient) Debug Report:\n";
			report += "    .NET Framework Version: " + Assembly
							 .GetEntryAssembly()
							 .GetReferencedAssemblies()
							 .Where(x => x.Name == "System.Core").First().Version.ToString() + "\n";
			report += "    Version of the API: 1.0.0\n";
			report += "    SDK Package Version: 1.0.0\n";

			return report;
		}
	}
}
