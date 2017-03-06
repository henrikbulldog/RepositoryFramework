using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RepositoryFramework
{
    public static class TypeExtensions
    {
        public static bool CheckPropertyName(this Type type, string property, out string validatedPropertyName)
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

        public static bool CheckPropertyPath(this Type type, string path, out string validatedPath)
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
