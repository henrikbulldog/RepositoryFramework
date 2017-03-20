using RepositoryFramework.Interfaces;
using System.Collections.Generic;

namespace RepositoryFramework.Interfaces
{
  public class QueryResult<TEntity> : IQueryResult<TEntity> where TEntity : class
    {
        public QueryResult(IEnumerable<TEntity> items, int totalCount)
        {
            Items = items;
            TotalCount = totalCount;
        }
        public IEnumerable<TEntity> Items { get; }

        public int TotalCount { get; }
    }
}
