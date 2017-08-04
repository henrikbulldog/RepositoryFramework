using System.IO;
using System.Threading.Tasks;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Binary large object repository
  /// </summary>
  public interface IBlobRepository :
    IDelete<BlobInfo>,
    IDeleteAsync<BlobInfo>,
    IDeleteMany<BlobInfo>,
    IDeleteManyAsync<BlobInfo>,
    IGetById<BlobInfo>,
    IGetByIdAsync<BlobInfo>,
    IFind<BlobInfo>,
    IFindAsync<BlobInfo>,
    IFindFilter<BlobInfo>,
    IFindFilterAsync<BlobInfo>
  {
    /// <summary>
    /// Upload blob
    /// </summary>
    /// <param name="entity">Blob info entity</param>
    /// <param name="stream">Upload stream</param>
    void Upload(BlobInfo entity, Stream stream);

    /// <summary>
    /// Upload blob
    /// </summary>
    /// <param name="entity">Blob info entity</param>
    /// <param name="stream">Upload stream</param>
    /// <returns>Task</returns>
    Task UploadAsync(BlobInfo entity, Stream stream);

    /// <summary>
    /// Download blob payload
    /// </summary>
    /// <param name="entity">Blob info entity</param>
    /// <param name="stream">Download stream</param>
    void Download(BlobInfo entity, Stream stream);

    /// <summary>
    /// Download blob payload
    /// </summary>
    /// <param name="entity">Blob info entity</param>
    /// <param name="stream">Download stream</param>
    /// <returns>Task</returns>
    Task DownloadAsync(BlobInfo entity, Stream stream);
  }
}
