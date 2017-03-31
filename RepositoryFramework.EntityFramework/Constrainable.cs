using System;
using System.Linq.Expressions;

namespace RepositoryFramework.EntityFramework
{
  /// <summary>
  /// Can get the name of a memeber property from an expression
  /// </summary>
  /// <typeparam name="TEntity">Entity type</typeparam>
  public class Constrainable<TEntity>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Constrainable{TEntity}"/> class
    /// </summary>
    public Constrainable()
    {
      ModelType = typeof(TEntity);
    }

    /// <summary>
    /// Gets or sets the model which will be queried.
    /// </summary>
    protected Type ModelType { get; set; }

    /// <summary>
    /// Get property name from expression
    /// </summary>
    /// <param name="exp">Property expression</param>
    /// <returns>Property name</returns>
    protected static string GetPropertyName(Expression<Func<TEntity, object>> exp)
    {
      var body = exp.Body as MemberExpression;

      if (body != null)
      {
        return body.Member.Name;
      }

      var ubody = (UnaryExpression)exp.Body;
      body = ubody.Operand as MemberExpression;

      return body?.Member.Name ?? string.Empty;
    }
  }
}
