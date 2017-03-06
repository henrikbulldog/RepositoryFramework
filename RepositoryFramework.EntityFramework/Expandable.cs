using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RepositoryFramework.EntityFramework
{
	public class Expandable<TEntity> : Constrainable<TEntity>, IExpandable<TEntity> where TEntity : class
	{
		/// <summary>
		/// List of reference properties to include
		/// </summary>
		public List<string> Includes { get; private set; }

		public IExpandable<TEntity> Include(List<string> propertyPaths)
		{
			Includes = new List<string>();
			if (propertyPaths == null) throw new ArgumentNullException(nameof(propertyPaths));
			foreach (var propertyPath in propertyPaths)
			{
				Include(propertyPath);
			}
			return this;
		}

		public IExpandable<TEntity> Include(string propertyPath)
		{
			if(Includes == null)
			{
				Includes = new List<string>();
			}
			if (!string.IsNullOrEmpty(propertyPath))
			{
				var validatedPropertyPath = propertyPath;
				if (!ModelType.CheckPropertyPath(propertyPath, out validatedPropertyPath))
				{
					throw new ArgumentException(
							string.Format("'{0}' is not a valid property path of '{1}'.",
							propertyPath, ModelType.FullName));
				}
				Includes.Add(validatedPropertyPath);
			}
			return this;
		}

    public IExpandable<TEntity> Include(Expression<Func<TEntity, object>> property)
    {
      var propertyPath = GetName(property);
      if (!ModelType.CheckPropertyPath(propertyPath, out propertyPath))
      {
        throw new ArgumentException(
            string.Format("'{0}' is not a valid property path of '{1}'.",
            propertyPath, ModelType.FullName));
      }
      Includes.Add(propertyPath);
      return this;
    }
  }
}
