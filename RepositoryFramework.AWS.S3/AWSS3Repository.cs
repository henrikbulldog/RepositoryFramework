using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.AWS.S3
{
  /// <summary>
  /// Amazon Web Services (AWS) Simple Storage Service (S3) repository
  /// </summary>
  /// <remarks>
  /// To use this repository you must have a valid AWS account
  /// To configure AWS credentials see http://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-creds.html
  /// To configure the Amazon S3 client in .Net Core see http://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-netcore.html
  /// </remarks>
  /// <typeparam name="TBlob">Blob type</typeparam>
  public class AWSS3Repository<TBlob> : IBlobRepository<TBlob>
    where TBlob : Blob, new()
  {
    private IAmazonS3 s3client = null;

    private string bucketName = null;

    private string bucketLocation = string.Empty;

    private CreateBlob createBlob = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="AWSS3Repository{TBlob}" /> class.
    /// </summary>
    /// <param name="s3client">AWS S3 client</param>
    /// <param name="bucketName">Bucket name</param>
    /// <param name="createBlob">Method to create new blobs</param>
    public AWSS3Repository(
      IAmazonS3 s3client,
      string bucketName,
      CreateBlob createBlob = null)
    {
      this.s3client = s3client;

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

      this.bucketName = bucketName;
      var getBucketLocationTask =
        s3client.GetBucketLocationAsync(new GetBucketLocationRequest
        {
          BucketName = this.bucketName
        });
      var doesS3BucketExistTask = s3client.DoesS3BucketExistAsync(this.bucketName);
      doesS3BucketExistTask.Wait();
      if (!doesS3BucketExistTask.Result)
      {
        // Note that CreateBucketRequest does not specify region. So bucket is
        // created in the region specified in the client.
        s3client.PutBucketAsync(bucketName).Wait();
      }

      getBucketLocationTask.Wait();
      if (getBucketLocationTask.Result != null)
      {
        bucketLocation = getBucketLocationTask.Result.Location;
      }
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
      PutObjectRequest request = new PutObjectRequest()
      {
        BucketName = bucketName,
        Key = entity.Id,
        InputStream = entity.CreateUploadStream()
      };
      await s3client.PutObjectAsync(request);
      entity.Uri = new Uri($"http://{bucketName}.s3-{bucketLocation}.amazonaws.com/{entity.Id}");
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
        await CreateAsync(entity);
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
      DeleteObjectRequest deleteObjectRequest =
          new DeleteObjectRequest
          {
            BucketName = bucketName,
            Key = entity.Id
          };

      await s3client.DeleteObjectAsync(deleteObjectRequest);
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
      DeleteObjectsRequest multiObjectDeleteRequest = new DeleteObjectsRequest();
      multiObjectDeleteRequest.BucketName = bucketName;

      foreach (var entity in entities)
      {
        multiObjectDeleteRequest.AddKey(entity.Id, null);
      }

      DeleteObjectsResponse response = await s3client.DeleteObjectsAsync(multiObjectDeleteRequest);
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
      var blob = createBlob(
        id.ToString(),
        -1,
        new Uri($"http://{bucketName}.s3-{bucketLocation}.amazonaws.com/{id}"));

      var request = new GetObjectRequest
      {
        BucketName = bucketName,
        Key = id.ToString(),
      };

      try
      {
        GetObjectResponse response = await s3client.GetObjectAsync(request);
        if (response.ResponseStream != null)
        {
          blob.Size = response.ResponseStream.Length;
          using (var inputStream = response.ResponseStream)
          {
            using (var outputStream = blob.CreateDownloadStream())
            {
              inputStream.CopyTo(outputStream);
            }
          }
        }
      }
      catch
      {
        return null;
      }

      return blob;
    }

    /// <summary>
    /// Get a list of entities
    /// </summary>
    /// <returns>Query result</returns>
    public IEnumerable<TBlob> Find()
    {
      var t = FindAsync(null);
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
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">Entity</param>
    public void Update(TBlob entity)
    {
      Create(entity);
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
      ListObjectsRequest request = new ListObjectsRequest
      {
        BucketName = bucketName,
        MaxKeys = 2,
        Prefix = prefix
      };
      var blobs = new List<TBlob>();

      do
      {
        ListObjectsResponse response = await s3client.ListObjectsAsync(request);

        // Process response.
        foreach (S3Object entry in response.S3Objects)
        {
          blobs.Add(createBlob(
            entry.Key,
            entry.Size,
            new Uri($"http://{bucketName}.s3-{bucketLocation}.amazonaws.com/{entry.Key}")));
        }

        // If response is truncated, set the marker to get the next
        // set of keys.
        if (response.IsTruncated)
        {
          request.Marker = response.NextMarker;
        }
        else
        {
          request = null;
        }
      }
      while (request != null);

      return blobs;
    }
  }
}
