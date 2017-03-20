using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.EntityFramework
{
	public class Repository<TEntity> :
		IRepository<TEntity>,
		IFindQueryable<TEntity>,
		IGetQueryable<TEntity>,
		IUnitOfWork where TEntity : class
	{
		protected DbContext DbContext { get; private set; }

		public Repository(DbContext dbContext)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
			DbContext = dbContext;
		}

		public virtual void Create(TEntity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));
			DbContext.Set<TEntity>().Add(entity);
		}

		public virtual void Delete(TEntity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));
			DbContext.Set<TEntity>().Remove(entity);
		}

		public virtual void Update(TEntity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));
			DbContext.Set<TEntity>().Update(entity);
		}

		public virtual IQueryResult<TEntity> Find(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter,
			IQueryConstraints<TEntity> constraints)
		{
			var query = Filter(filter);

			IEnumerable<TEntity> items;

			if (constraints != null)
			{
				items = constraints.ApplyTo(query).ToList();
			}
			else
			{
				items = query.ToList();
			}

			var count = query.Count();

			return new QueryResult<TEntity>(items, count);
		}

		private IQueryable<TEntity> Filter(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
		{
			IQueryable<TEntity> query;

			if (filter != null)
			{
				query = filter(DbContext.Set<TEntity>().AsQueryable());
			}
			else
			{
				query = DbContext.Set<TEntity>();
			}

			return query;
		}

		public virtual IQueryResult<TEntity> Find()
		{
			return Find(null, null);
		}

		public virtual IQueryResult<TEntity> Find(IQueryConstraints<TEntity> constraints)
		{
			return Find(null, constraints);
		}

		public IQueryResult<TEntity> Find(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
		{
			return Find(filter, null);
		}

		public virtual void Dispose()
		{

		}

		public virtual void SaveChanges()
		{
			DbContext.SaveChanges();
		}

		public virtual void DetachAll()
		{
			foreach (var entityEntry in DbContext.ChangeTracker.Entries().ToArray())
			{
				if (entityEntry.Entity != null)
				{
					entityEntry.State = EntityState.Detached;
				}
			}
		}

		public TEntity GetById(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
		{
			return GetById(filter, null);
		}

		public TEntity GetById(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter, IExpandable<TEntity> includes)
		{
			var query = Filter(filter);
			if (includes != null)
			{
				query = includes.AddExpansion(query);
			}

			return query.SingleOrDefault();
		}

	}
}
