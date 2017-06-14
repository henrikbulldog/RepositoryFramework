using System;
using System.IO;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Binary large object stored to a file
  /// </summary>
  public class FileBlob : Blob
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="FileBlob" /> class.
    /// </summary>
    public FileBlob()
      : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileBlob" /> class.
    /// </summary>
    /// <param name="id">Id or name of the object</param>
    /// <param name="size">Size of the object</param>
    /// <param name="uri">Uri to access the object</param>
    /// <param name="downloadFolder">Folder to contain downloaded file</param>
    public FileBlob(string id, long size, Uri uri, string downloadFolder = ".")
      : base(id, size, uri)
    {
      DownloadFolder = downloadFolder;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileBlob" /> class.
    /// </summary>
    /// <param name="id">Id or name of the object</param>
    /// <param name="uploadFilePath">Full path to a file to be uploaded</param>
    public FileBlob(string id, string uploadFilePath = null)
      : base(id)
    {
      if (string.IsNullOrWhiteSpace(uploadFilePath))
      {
        uploadFilePath = id;
      }

      UploadFilePath = uploadFilePath;
    }

    /// <summary>
    /// Gets or sets Folder to contain downloaded file
    /// </summary>
    public virtual string DownloadFolder { get; set; } = ".";

    /// <summary>
    /// Gets or sets Full path to a file to be uploaded
    /// </summary>
    public virtual string UploadFilePath { get; set; }

    /// <summary>
    /// Gets the full file path
    /// </summary>
    public virtual string FilePath
    {
      get
      {
        return Path.Combine(DownloadFolder, Id);
      }
    }

    /// <summary>
    /// Creates a stream to download the object. Please wrap the stream into a using block to make sure that all IO resources are closed upon completion of the operation
    /// </summary>
    /// <returns>Upload stream</returns>
    public override Stream CreateDownloadStream()
    {
      FileInfo fileInfo = new FileInfo(FilePath);
      if (!Directory.Exists(fileInfo.DirectoryName))
      {
        Directory.CreateDirectory(fileInfo.Directory.FullName);
      }

      return new FileStream(fileInfo.FullName, FileMode.Create);
    }

    /// <summary>
    /// Creates a stream to upload the object. Please wrap the stream into a using block to make sure that all IO resources are closed upon completion of the operation
    /// </summary>
    /// <returns>Upload stream</returns>
    public override Stream CreateUploadStream()
    {
      return new FileStream(UploadFilePath, FileMode.Open);
    }

    /// <summary>
    /// Convert to string
    /// </summary>
    /// <returns>String</returns>
    public override string ToString()
    {
      return $"{Id}: {FilePath}";
    }
  }
}
