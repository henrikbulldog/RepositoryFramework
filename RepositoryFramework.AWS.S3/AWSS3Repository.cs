using System;
using System.Collections.Generic;
using System.IO;
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
  public class AWSS3Repository : IBlobRepository
  {
    private IAmazonS3 s3client = null;

    private string bucketName = null;

    private string bucketLocation = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AWSS3Repository" /> class.
    /// </summary>
    /// <param name="s3client">AWS S3 client</param>
    /// <param name="bucketName">Bucket name</param>
    public AWSS3Repository(
      IAmazonS3 s3client,
      string bucketName)
    {
      this.s3client = s3client;
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
    public BlobInfo GetById(object id)
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
    public async Task<BlobInfo> GetByIdAsync(object id)
    {
      var blob = new BlobInfo(
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
        blob.Size = response.ContentLength;
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
    public IEnumerable<BlobInfo> Find()
    {
      var t = FindAsync(null);
      t.WaitSync();
      return t.Result;
    }

    /// <summary>
    /// Get a list of entities
    /// </summary>
    /// <returns>Query result</returns>
    public async Task<IEnumerable<BlobInfo>> FindAsync()
    {
      return await FindAsync(null);
    }

    /// <summary>
    /// Filters a collection of entities from a filter definition
    /// </summary>
    /// <param name="prefix">Filter objects by prefix</param>
    /// <returns>Filtered collection of entities</returns>
    public IEnumerable<BlobInfo> Find(string prefix)
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
    public async Task<IEnumerable<BlobInfo>> FindAsync(string prefix)
    {
      ListObjectsRequest request = new ListObjectsRequest
      {
        BucketName = bucketName,
        MaxKeys = 2,
        Prefix = prefix
      };
      var blobs = new List<BlobInfo>();

      do
      {
        ListObjectsResponse response = await s3client.ListObjectsAsync(request);

        // Process response.
        foreach (S3Object entry in response.S3Objects)
        {
          blobs.Add(new BlobInfo(
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
      PutObjectRequest request = new PutObjectRequest()
      {
        BucketName = bucketName,
        Key = entity.Id,
        InputStream = stream
      };
      await s3client.PutObjectAsync(request);
      entity.Uri = new Uri($"http://{bucketName}.s3-{bucketLocation}.amazonaws.com/{entity.Id}");
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
      var request = new GetObjectRequest
      {
        BucketName = bucketName,
        Key = entity.Id.ToString(),
      };

      GetObjectResponse response = await s3client.GetObjectAsync(request);
      if (response.ResponseStream != null)
      {
        entity.Size = response.ResponseStream.Length;
        using (var inputStream = response.ResponseStream)
        {
          inputStream.CopyTo(stream);
        }
      }
    }
  }
}
