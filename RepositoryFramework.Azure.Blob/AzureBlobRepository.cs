using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.Azure.Blob
{
  /// <summary>
  /// Microsoft Azure Blob Storage repository
  /// </summary>
  public class AzureBlobRepository : IBlobRepository
  {
    private CloudBlobContainer container = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobRepository" /> class.
    /// </summary>
    /// <param name="container">Azure Blob Storage conainer</param>
    public AzureBlobRepository(
      CloudBlobContainer container)
    {
      this.container = container;
      container.CreateIfNotExistsAsync().WaitSync();
    }

    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public void Delete(BlobInfo entity)
    {
      DeleteAsync(entity).WaitSync();
    }

    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Task</returns>
    public async Task DeleteAsync(BlobInfo entity)
    {
      CloudBlockBlob blockBlob = container.GetBlockBlobReference(entity.Id);
      await blockBlob.DeleteIfExistsAsync();
    }

    /// <summary>
    /// Delete a list of existing entities
    /// </summary>
    /// <param name="entities">Entity list</param>
    public void DeleteMany(IEnumerable<BlobInfo> entities)
    {
      DeleteManyAsync(entities).WaitSync();
    }

    /// <summary>
    /// Delete a list of existing entities
    /// </summary>
    /// <param name="entities">Entity list</param>
    /// <returns>Task</returns>
    public async Task DeleteManyAsync(IEnumerable<BlobInfo> entities)
    {
      foreach (var entity in entities)
      {
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(entity.Id);
        await blockBlob.DeleteIfExistsAsync();
      }
    }

    /// <summary>
    /// Get a list of entities
    /// </summary>
    /// <returns>Query result</returns>
    public IEnumerable<Interfaces.BlobInfo> Find()
    {
      var t = FindAsync();
      t.WaitSync();
      return t.Result;
    }

    /// <summary>
    /// Get a list of entities
    /// </summary>
    /// <returns>Query result</returns>
    public async Task<IEnumerable<Interfaces.BlobInfo>> FindAsync()
    {
      return await FindAsync(null);
    }

    /// <summary>
    /// Filters a collection of entities from a filter definition
    /// </summary>
    /// <param name="prefix">Filter objects by prefix</param>
    /// <returns>Filtered collection of entities</returns>
    public IEnumerable<Interfaces.BlobInfo> Find(string prefix)
    {
      var t = FindAsync(prefix);
      t.WaitSync();
      return t.Result;
    }

    /// <summary>
    /// Filters a collection of entities from a filter definition
    /// </summary>
    /// <param name="prefix">Filter objects by prefix</param>
    /// <returns>Filtered collection of entities</returns>
    public async Task<IEnumerable<Interfaces.BlobInfo>> FindAsync(string prefix)
    {
      BlobContinuationToken continuationToken = null;
      BlobResultSegment resultSegment = null;
      var blobs = new List<Interfaces.BlobInfo>();
      do
      {
        resultSegment = await container.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.Metadata, null, continuationToken, null, null);
        if (resultSegment != null && resultSegment.Results != null)
        {
          foreach (var cloudBlob in resultSegment.Results)
          {
            var block = cloudBlob as CloudBlockBlob;
            if (block != null)
            {
              var blob = new Interfaces.BlobInfo();
              blob.Id = block.Name;
              blob.Size = block.Properties.Length;
              blob.Uri = block.Uri;
              blobs.Add(blob);
            }
          }

          continuationToken = resultSegment.ContinuationToken;
        }
        else
        {
          continuationToken = null;
        }
      }
      while (continuationToken != null);

      return blobs;
    }

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="id">Filter to find a single item</param>
    /// <returns>Entity</returns>
    public Interfaces.BlobInfo GetById(object id)
    {
      var t = GetByIdAsync(id);
      t.WaitSync();
      return t.Result;
    }

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="id">Filter to find a single item</param>
    /// <returns>Entity</returns>
    public async Task<Interfaces.BlobInfo> GetByIdAsync(object id)
    {
      try
      {
        CloudBlockBlob block = container.GetBlockBlobReference(id.ToString());
        await block.FetchAttributesAsync();
        var blob = new Interfaces.BlobInfo();
        blob.Id = block.Name;
        blob.Size = block.Properties.Length;
        blob.Uri = block.Uri;
        return blob;
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// Download blob payload
    /// </summary>
    /// <param name="entity">Blob info entity</param>
    /// <param name="stream">Download stream</param>
    public void Download(BlobInfo entity, Stream stream)
    {
      DownloadAsync(entity, stream).WaitSync();
    }

    /// <summary>
    /// Download blob payload
    /// </summary>
    /// <param name="entity">Blob info entity</param>
    /// <param name="stream">Download stream</param>
    /// <returns>Task</returns>
    public async Task DownloadAsync(BlobInfo entity, Stream stream)
    {
      if (entity != null)
      {
        CloudBlockBlob block = container.GetBlockBlobReference(entity.Id);
        if (stream != null)
        {
          await block.DownloadToStreamAsync(stream);
        }
      }
    }

    /// <summary>
    /// Upload blob
    /// </summary>
    /// <param name="entity">Blob info entity</param>
    /// <param name="stream">Upload stream</param>
    public void Upload(BlobInfo entity, Stream stream)
    {
      UploadAsync(entity, stream).WaitSync();
    }

    /// <summary>
    /// Upload blob
    /// </summary>
    /// <param name="entity">Blob info entity</param>
    /// <param name="stream">Upload stream</param>
    /// <returns>Task</returns>
    public async Task UploadAsync(BlobInfo entity, Stream stream)
    {
      CloudBlockBlob block = container.GetBlockBlobReference(entity.Id);
      await block.UploadFromStreamAsync(stream);
      await block.FetchAttributesAsync();
      entity.Size = block.Properties.Length;
      entity.Uri = block.Uri;
    }
  }
}
