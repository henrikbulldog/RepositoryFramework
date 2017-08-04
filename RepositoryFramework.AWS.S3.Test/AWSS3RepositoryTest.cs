using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Configuration;
using Amazon.S3;
using Moq;
using RepositoryFramework.Interfaces;
using RepositoryFramework.AWS.S3;
using Xunit;
using Amazon.S3.Model;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Text;

namespace RepositoryFramework.Azure.Blob.Test
{
  public class AWSS3RepositoryTest
  {
    /// <summary>
    /// Set to false to test AWS Simple Storage Service
    /// An AWS credential file must be present in C:\Users\[user name]\.aws with this content:
    /// [local-test-profile]
    /// aws_access_key_id = key
    /// aws_secret_access_key = key 
    /// 
    /// appSettings múst contaion valid AWS settings (check region)
    /// 
    /// bucketName must be set to a valid bucket name
    /// </summary>
    private bool useMockContainer = true;

    private const string bucketName = "foss.enablement.s3";

    [Fact]
    public void Upload_And_Download()
    {
      using (var s3client = GetAWSS3Client())
      {
        var r = new AWSS3Repository(s3client, bucketName);
        var blob = new BlobInfo($"AWSS3RepositoryTest/Upload_And_Download/{Guid.NewGuid()}");
        var payload = "payload";
        Upload(blob, payload, r);
        var result = Download(blob.Id, r);

        if (!useMockContainer)
        {
          Assert.Equal(payload, result);
        }

        r.Delete(blob);
      }
    }

    private void Upload(BlobInfo blob, string payload, IBlobRepository repository)
    {
      var uploadBuffer = Encoding.UTF8.GetBytes(payload);
      using (var uploadStream = new MemoryStream(uploadBuffer))
      {
        repository.Upload(blob, uploadStream);
      }
    }

    private string Download(string id, IBlobRepository repository)
    {
      var blob = repository.GetById(id);
      if (blob != null && blob.Size > 0)
      {
        var downloadBuffer = new byte[blob.Size];
        using (var downloadStream = new MemoryStream(downloadBuffer))
        {
          repository.Download(blob, downloadStream);
        }
        return Encoding.UTF8.GetString(downloadBuffer);
      }
      return string.Empty;
    }

    [Fact]
    public void Find()
    {
      using (var s3client = GetAWSS3Client())
      {
        var r = new AWSS3Repository(s3client, bucketName);
        var blobs = new List<BlobInfo>
      {
        new BlobInfo("AWSS3RepositoryTest.Find/file1.ext"),
        new BlobInfo("AWSS3RepositoryTest.Find/folder1/file2.ext"),
        new BlobInfo("AWSS3RepositoryTest.Find/folder1/file3.ext")
      };
        Upload(blobs[0], "payload1", r);
        Upload(blobs[1], "payload2", r);
        Upload(blobs[2], "payload3", r);
        if (!useMockContainer)
        {
          Assert.Equal(3, r.Find("AWSS3RepositoryTest.Find/").Count());
          Assert.Equal(2, r.Find("AWSS3RepositoryTest.Find/folder1/").Count());
        }
        r.DeleteMany(blobs);
      }
    }

    [Fact]
    public void GetById_Not_Found()
    {
      using (var s3client = GetAWSS3Client())
      {
        var r = new AWSS3Repository(s3client, bucketName);
        var id = "AzureBlobRepositoryTest.GetById_Not_Found.ext";
        var result = r.GetById(id);
        if (!useMockContainer)
        {
          Assert.Null(result);
        }
      }
    }

    public static IConfigurationRoot Configuration { get; set; }

    private IAmazonS3 GetAWSS3Client()
    {
      if (useMockContainer)
      {
        return GetAWSS3ClientMock().Object;
      }
      var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json");

      Configuration = builder.Build();
      var options = Configuration.GetAWSOptions();

      return options.CreateServiceClient<IAmazonS3>();
    }

    private Mock<IAmazonS3> GetAWSS3ClientMock()
    {
      var s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Loose);
      var getObjectResponseMock = new Mock<GetObjectResponse>(MockBehavior.Loose);
      s3ClientMock
        .Setup(c => c.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
        .Returns(Task.FromResult(getObjectResponseMock.Object));
      return s3ClientMock;
    }
  }
}
