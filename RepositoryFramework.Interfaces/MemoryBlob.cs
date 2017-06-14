using System.IO;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Large binary object stored in memory
  /// </summary>
  public class MemoryBlob : Blob
  {
    private byte[] buffer = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryBlob" /> class.
    /// </summary>
    public MemoryBlob()
      : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryBlob" /> class.
    /// </summary>
    /// <param name="id">Id or name ofthe object</param>
    /// <param name="payload">Object payload</param>
    public MemoryBlob(string id, string payload)
      : base(id)
    {
      if (!string.IsNullOrWhiteSpace(payload))
      {
        buffer = System.Text.Encoding.UTF8.GetBytes(payload);
        Size = Payload.Length;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryBlob" /> class.
    /// </summary>
    /// <param name="id">Id or name ofthe object</param>
    /// <param name="size">Size of the object</param>
    public MemoryBlob(string id, long size)
      : base(id, size)
    {
    }

    /// <summary>
    /// Gets the object payload
    /// </summary>
    public virtual string Payload
    {
      get
      {
        if (buffer == null)
        {
          return null;
        }

        return System.Text.Encoding.UTF8.GetString(buffer);
      }
    }

    /// <summary>
    /// Creates a stream to download the object. Please wrap the stream into a using block to make sure that all IO resources are closed upon completion of the operation
    /// </summary>
    /// <returns>Upload stream</returns>
    public override Stream CreateDownloadStream()
    {
      buffer = new byte[Size > 0 ? Size : 1024];
      return new MemoryStream(buffer);
    }

    /// <summary>
    /// Creates a stream to upload the object. Please wrap the stream into a using block to make sure that all IO resources are closed upon completion of the operation
    /// </summary>
    /// <returns>Upload stream</returns>
    public override Stream CreateUploadStream()
    {
      return new MemoryStream(buffer);
    }

    /// <summary>
    /// Convert to string
    /// </summary>
    /// <returns>String</returns>
    public override string ToString()
    {
      return $"{Id}: {Payload}";
    }
  }
}
