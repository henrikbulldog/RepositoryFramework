namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Binary large object repository
  /// </summary>
  /// <typeparam name="TBlob">Blob type</typeparam>
  public interface IBlobRepository<TBlob> : IRepository<TBlob>, IFindFilter<TBlob>, IFindFilterAsync<TBlob>
    where TBlob : Blob, new()
  {
  }
}
