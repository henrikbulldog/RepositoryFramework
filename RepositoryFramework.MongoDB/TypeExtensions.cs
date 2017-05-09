using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RepositoryFramework.MongoDB
{
  /// <summary>
  /// Extensions to the <see cref="Type"/> class for checking property names and paths
  /// </summary>
  public static class TypeExtensions
  {
    /// <summary>
    /// Chech property name
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="property">Property name</param>
    /// <param name="validatedPropertyName">Validated property name, casing is corrected</param>
    /// <returns>Success</returns>
    public static bool TryCheckPropertyName(this Type type, string property, out string validatedPropertyName)
    {
      validatedPropertyName = property;
      var pi = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
      if (pi == null)
      {
        return false;
      }

      validatedPropertyName = pi.Name;
      return true;
    }

    /// <summary>
    /// Check property path
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="path">Path to a property or a property of a related type</param>
    /// <param name="validatedPath">Validated path, property name casing is corrected</param>
    /// <returns>Success</returns>
    public static bool TryCheckPropertyPath(this Type type, string path, out string validatedPath)
    {
      validatedPath = path;
      var properties = path.Split('.');
      List<string> validated = new List<string>();

      foreach (var property in properties)
      {
        var pi = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (pi == null)
        {
          return false;
        }

        validated.Add(pi.Name);
        if (pi.PropertyType.IsArray)
        {
          type = pi.PropertyType.GetElementType();
        }
        else if (pi.PropertyType.IsConstructedGenericType)
        {
          type = pi.PropertyType.GetGenericArguments().Single();
        }
        else
        {
          type = pi.PropertyType;
        }
      }

      validatedPath = string.Join(".", validated);
      return true;
    }
  }
}
