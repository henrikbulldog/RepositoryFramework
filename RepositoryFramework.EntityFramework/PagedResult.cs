using RepositoryFramework.Interfaces;
using System.Collections.Generic;

namespace RepositoryFramework.EntityFramework
{
    public class PagedResult<TEntity> : IQueryResult<TEntity> where TEntity : class
    {
        public PagedResult(IEnumerable<TEntity> items, int totalCount)
        {
            Items = items;
            TotalCount = totalCount;
        }
        public IEnumerable<TEntity> Items { get; }

        public int TotalCount { get; }
    }
}
