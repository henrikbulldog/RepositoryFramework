using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Base class for generic repositories
  /// </summary>
  /// <typeparam name="TEntity">Entoty type</typeparam>
  public abstract class GenericRepositoryBase<TEntity>
    where TEntity : class
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="GenericRepositoryBase{TEntity}" /> class.
    /// </summary>
    /// <param name="idProperty">Id property expression</param>
    public GenericRepositoryBase(Expression<Func<TEntity, object>> idProperty = null)
    {
      if (idProperty != null)
      {
        IdPropertyName = GetPropertyName(idProperty);
      }
      else
      {
        IdPropertyName = FindIdProperty();
      }
    }

    /// <summary>
    /// Gets entity type
    /// </summary>
    protected static Type EntityType { get; private set; } = typeof(TEntity);

    /// <summary>
    /// Gets entity type name
    /// </summary>
    protected static string EntityTypeName { get; private set; } = typeof(TEntity).Name;

    /// <summary>
    /// Gets entity database columns: all value type properties
    /// </summary>
    protected static string[] EntityColumns { get; private set; } = typeof(TEntity).GetProperties()
      .Where(p => (p.PropertyType.GetTypeInfo().GetInterface("IEnumerable") == null
      && p.PropertyType.GetTypeInfo().GetInterface("ICollection") == null
      && !p.PropertyType.GetTypeInfo().IsClass)
      || p.PropertyType.IsAssignableFrom(typeof(string)))
      .Select(p => p.Name)
      .ToArray();

    /// <summary>
    /// Gets or sets entity Id property
    /// </summary>
    protected string IdPropertyName { get; private set; }

    /// <summary>
    /// Find the Id property of the entity type looking for properties with name Id or (entity type name)Id
    /// </summary>
    /// <returns>Id property name or null if none could befound</returns>
    protected static string FindIdProperty()
    {
      var idProperty = EntityColumns
        .FirstOrDefault(c => c.ToLower() == $"{EntityTypeName.ToLower()}id");

      if (idProperty == null)
      {
        idProperty = EntityColumns
          .FirstOrDefault(c => c.ToLower() == "id");
      }

      return idProperty;
    }

    /// <summary>
    /// Get the name of a property from an expression
    /// </summary>
    /// <param name="propertyExpression">Property expression</param>
    /// <returns>Property name</returns>
    protected static string GetPropertyName(Expression<Func<TEntity, object>> propertyExpression)
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
    /// Chech property name
    /// </summary>
    /// <param name="property">Property name</param>
    /// <param name="validatedPropertyName">Validated property name, casing is corrected</param>
    /// <returns>Success</returns>
    protected static bool TryCheckPropertyName(string property, out string validatedPropertyName)
    {
      validatedPropertyName = property;
      var pi = EntityType.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
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
    /// <param name="path">Path to a property or a property of a related type</param>
    /// <param name="validatedPath">Validated path, property name casing is corrected</param>
    /// <returns>Success</returns>
    protected static bool TryCheckPropertyPath(string path, out string validatedPath)
    {
      validatedPath = path;
      var properties = path.Split('.');
      List<string> validated = new List<string>();

      var type = EntityType;
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

    /// <summary>
    /// Make sure that the property exists in the model.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="validatedName">Validated name</param>
    protected static void ValidatePropertyName(string name, out string validatedName)
    {
      validatedName = name;
      if (name == null)
      {
        throw new ArgumentNullException(nameof(name));
      }

      if (!TryCheckPropertyName(name, out validatedName))
      {
        throw new ArgumentException(
          string.Format(
            "'{0}' is not a public property of '{1}'.",
            name,
            EntityTypeName));
      }
    }

    /// <summary>
    /// Convert parameter collection to an object
    /// </summary>
    /// <returns>Object</returns>
    protected static object ToObject(IDictionary<string, Object> parameters)
    {
      var dynamicObject = new ExpandoObject() as IDictionary<string, Object>;
      foreach (var parameter in parameters)
      {
        dynamicObject.Add(parameter.Key, parameter.Value);
      }
      return dynamicObject;
    }

    /// <summary>
    /// Gets a property selector expression from the property name
    /// </summary>
    /// <param name="propertyName">Property name</param>
    /// <returns>Property selector expression </returns>
    public static Expression<Func<TEntity, object>> GetPropertySelector(string propertyName)
    {
      var arg = Expression.Parameter(typeof(TEntity), "x");
      var property = Expression.Property(arg, propertyName);
      //return the property as object
      var conv = Expression.Convert(property, typeof(object));
      var exp = Expression.Lambda<Func<TEntity, object>>(conv, new ParameterExpression[] { arg });
      return exp;
    }
  }
}
