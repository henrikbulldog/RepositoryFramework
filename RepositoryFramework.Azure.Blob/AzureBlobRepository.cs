using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.Azure.Blob
{
  /// <summary>
  /// Microsoft Azure Blob Storage repository
  /// </summary>
  /// <typeparam name="TBlob">Blob type</typeparam>
  public class AzureBlobRepository<TBlob> : IBlobRepository<TBlob>
    where TBlob : Interfaces.Blob, new()
  {
    private CreateBlob createBlob = null;

    private CloudBlobContainer container = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobRepository{TBlob}" /> class.
    /// </summary>
    /// <param name="container">Azure Blob Storage conainer</param>
    /// <param name="createBlob">Method to create new blobs</param>
    public AzureBlobRepository(
      CloudBlobContainer container,
      CreateBlob createBlob = null)
    {
      if (createBlob == null)
      {
        this.createBlob = (id, size, uri) =>
        {
          var b = new TBlob();
          b.Id = id;
          b.Size = size;
          b.Uri = uri;
          return b;
        };
      }
      else
      {
        this.createBlob = createBlob;
      }

      this.container = container;
      container.CreateIfNotExistsAsync().WaitSync();
    }

    /// <summary>
    /// Creates a new blob
    /// </summary>
    /// <param name="id">Id or name of the object</param>
    /// <param name="size">Size of the object</param>
    /// <param name="uri">Uri to access the object</param>
    /// <returns>New blob instance</returns>
    public delegate TBlob CreateBlob(string id, long size, Uri uri);

    /// <summary>
    /// Create a new entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public void Create(TBlob entity)
    {
      CreateAsync(entity).WaitSync();
    }

    /// <summary>
    /// Create a new entity
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Task</returns>
    public async Task CreateAsync(TBlob entity)
    {
      CloudBlockBlob blockBlob = container.GetBlockBlobReference(entity.Id);
      entity.Uri = blockBlob.Uri;
      using (var stream = entity.CreateUploadStream())
      {
        await blockBlob.UploadFromStreamAsync(stream);
      }
    }

    /// <summary>
    /// Create a list of new entities
    /// </summary>
    /// <param name="entities">List of entities</param>
    public void CreateMany(IEnumerable<TBlob> entities)
    {
      CreateManyAsync(entities).WaitSync();
    }

    /// <summary>
    /// Create a list of new entities
    /// </summary>
    /// <param name="entities">List of entities</param>
    /// <returns>Task</returns>
    public async Task CreateManyAsync(IEnumerable<TBlob> entities)
    {
      foreach (var entity in entities)
      {
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(entity.Id);
        entity.Uri = blockBlob.Uri;
        using (var stream = entity.CreateUploadStream())
        {
          await blockBlob.UploadFromStreamAsync(stream);
        }
      }
    }

    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public void Delete(TBlob entity)
    {
      DeleteAsync(entity).WaitSync();
    }

    /// <summary>
    /// Delete an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Task</returns>
    public async Task DeleteAsync(TBlob entity)
    {
      CloudBlockBlob blockBlob = container.GetBlockBlobReference(entity.Id);
      await blockBlob.DeleteIfExistsAsync();
    }

    /// <summary>
    /// Delete a list of existing entities
    /// </summary>
    /// <param name="entities">Entity list</param>
    public void DeleteMany(IEnumerable<TBlob> entities)
    {
      DeleteManyAsync(entities).WaitSync();
    }

    /// <summary>
    /// Delete a list of existing entities
    /// </summary>
    /// <param name="entities">Entity list</param>
    /// <returns>Task</returns>
    public async Task DeleteManyAsync(IEnumerable<TBlob> entities)
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
    public IEnumerable<TBlob> Find()
    {
      var t = FindAsync();
      t.WaitSync();
      return t.Result;
    }

    /// <summary>
    /// Get a list of entities
    /// </summary>
    /// <returns>Query result</returns>
    public async Task<IEnumerable<TBlob>> FindAsync()
    {
      return await FindAsync(null);
    }

    /// <summary>
    /// Filters a collection of entities from a filter definition
    /// </summary>
    /// <param name="prefix">Filter objects by prefix</param>
    /// <returns>Filtered collection of entities</returns>
    public IEnumerable<TBlob> Find(string prefix)
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
    public async Task<IEnumerable<TBlob>> FindAsync(string prefix)
    {
      BlobContinuationToken continuationToken = null;
      BlobResultSegment resultSegment = null;
      var blobs = new List<TBlob>();
      do
      {
        resultSegment = await container.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.Metadata, null, continuationToken, null, null);
        if (resultSegment != null && resultSegment.Results != null)
        {
          foreach (var blob in resultSegment.Results)
          {
            var block = blob as CloudBlockBlob;
            if (block != null)
            {
              blobs.Add(createBlob(block.Name, block.Properties.Length, block.Uri));
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
    public TBlob GetById(object id)
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
    public async Task<TBlob> GetByIdAsync(object id)
    {
      try
      {
        CloudBlockBlob block = container.GetBlockBlobReference(id.ToString());
        await block.FetchAttributesAsync();
        var blob = createBlob(id.ToString(), block.Properties.Length, block.Uri);
        using (var stream = blob.CreateDownloadStream())
        {
          await block.DownloadToStreamAsync(stream);
        }

        return blob;
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public void Update(TBlob entity)
    {
      CreateAsync(entity).WaitSync();
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Task</returns>
    public async Task UpdateAsync(TBlob entity)
    {
      await CreateAsync(entity);
    }
  }
}
