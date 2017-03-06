using System;
using System.Linq.Expressions;

namespace RepositoryFramework.EntityFramework
{
	public class Sortable<TEntity> : Constrainable<TEntity>, ISortable<TEntity> where TEntity : class
	{
		/// <summary>
		/// Gets the kind of sort order
		/// </summary>
		public SortOrder SortOrder { get; private set; }

		/// <summary>
		/// Gets property name for the property to sort by.
		/// </summary>
		public string SortPropertyName { get; private set; }

		/// <summary>
		/// Sort ascending by a property
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns>Current instance</returns>
		public ISortable<TEntity> SortBy(string propertyName)
		{
			if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
			ValidatePropertyName(propertyName, out propertyName);

			SortOrder = SortOrder.Ascending;
			SortPropertyName = propertyName;
			return this;
		}

		/// <summary>
		/// Sort descending by a property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns>Current instance</returns>
		public ISortable<TEntity> SortByDescending(string propertyName)
		{
			if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
			ValidatePropertyName(propertyName, out propertyName);

			SortOrder = SortOrder.Descending;
			SortPropertyName = propertyName;
			return this;
		}

		/// <summary>
		/// Property to sort by (ascending)
		/// </summary>
		/// <param name="property">The property.</param>
		public ISortable<TEntity> SortBy(Expression<Func<TEntity, object>> property)
		{
			if (property == null) throw new ArgumentNullException(nameof(property));

			var name = GetName(property);
			SortBy(name);
			return this;
		}

		/// <summary>
		/// Property to sort by (descending)
		/// </summary>
		/// <param name="property">The property</param>
		public ISortable<TEntity> SortByDescending(Expression<Func<TEntity, object>> property)
		{
			if (property == null) throw new ArgumentNullException(nameof(property));
			var name = GetName(property);
			SortByDescending(name);
			return this;
		}
		/// <summary>
		/// Make sure that the property exists in the model.
		/// </summary>
		/// <param name="name">The name.</param>
		protected virtual void ValidatePropertyName(string name, out string validatedName)
		{
			validatedName = name;
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (!ModelType.CheckPropertyName(name, out validatedName))
			{
				throw new ArgumentException(
						string.Format("'{0}' is not a public property of '{1}'.",
						name, ModelType.FullName));
			}
		}
	}
}