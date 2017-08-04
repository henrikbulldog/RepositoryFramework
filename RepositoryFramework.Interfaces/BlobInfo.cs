using System;
using System.IO;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// A binary large object
  /// </summary>
  public class BlobInfo
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="BlobInfo" /> class.
    /// </summary>
    public BlobInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlobInfo" /> class.
    /// </summary>
    /// <param name="id">Id or name of the object</param>
    /// <param name="size">Size of the object</param>
    /// <param name="uri">Uri to access the object</param>
    public BlobInfo(string id, long size = -1, Uri uri = null)
    {
      Id = id;
      Size = size;
      Uri = uri;
    }

    /// <summary>
    /// Gets or sets Id or name of the object
    /// </summary>
    public virtual string Id { get; set; }

    /// <summary>
    /// Gets or sets Size of the object
    /// </summary>
    public virtual long Size { get; set; }

    /// <summary>
    /// Gets or sets Uri to access the object
    /// </summary>
    public virtual Uri Uri { get; set; }

    /// <summary>
    /// Creates a stream to upload the object. Please wrap the stream into a using block to make sure that all IO resources are closed upon completion of the operation
    /// </summary>
    /// <returns>Upload stream</returns>
    public virtual Stream CreateUploadStream()
    {
      return null;
    }

    /// <summary>
    /// Creates a stream to download the object. Please wrap the stream into a using block to make sure that all IO resources are closed upon completion of the operation
    /// </summary>
    /// <returns>Upload stream</returns>
    public virtual Stream CreateDownloadStream()
    {
      return null;
    }
  }
}
