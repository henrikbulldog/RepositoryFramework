using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RepositoryFramework.EntityFramework
{
  /// <summary>
  /// Can get the name of a memeber property from an expression
  /// </summary>
  /// <typeparam name="TEntity"></typeparam>
  public class Constrainable<TEntity>
  {
    public Constrainable()
    {
      ModelType = typeof(TEntity);
    }

    /// <summary>
    /// Gets model which will be queried.
    /// </summary>
    protected Type ModelType { get; set; }

    /// <summary>
    /// Get property name from expression
    /// </summary>
    /// <param name="exp">Property expression</param>
    protected static string GetName(Expression<Func<TEntity, object>> exp)
    {
      var body = exp.Body as MemberExpression;

      if (body != null) return body.Member.Name;

      var ubody = (UnaryExpression)exp.Body;
      body = ubody.Operand as MemberExpression;

      return body?.Member.Name ?? string.Empty;
    }
  }
}
