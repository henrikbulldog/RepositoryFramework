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
    public void Create()
    {
      using (var s3client = GetAWSS3Client())
      {
        var r = new AWSS3Repository<MemoryBlob>(s3client, bucketName);
        var blob = new MemoryBlob("AWSS3RepositoryTest.Create.ext", "payload");
        r.Create(blob);
        r.Delete(blob);
      }
    }

    [Fact]
    public void CreateMany()
    {
      using (var s3client = GetAWSS3Client())
      {
        var r = new AWSS3Repository<MemoryBlob>(s3client, bucketName);
        var blobs = new List<MemoryBlob>
        {
          new MemoryBlob("AWSS3RepositoryTest.CreateMany1.ext", "payload"),
          new MemoryBlob("AWSS3RepositoryTest.CreateMany2.ext", "payload"),
          new MemoryBlob("AWSS3RepositoryTest.CreateMany3.ext", "payload")
        };
        r.CreateMany(blobs);
        r.DeleteMany(blobs);
      }
    }

    [Fact]
    public void Update()
    {
      using (var s3client = GetAWSS3Client())
      {
        var r = new AWSS3Repository<MemoryBlob>(s3client, bucketName);
        var blob = new MemoryBlob("AWSS3RepositoryTest.Update.ext", "payload");
        r.Create(blob);
        r.Update(blob);
        r.Delete(blob);
      }
    }

    [Fact]
    public void Find()
    {
      using (var s3client = GetAWSS3Client())
      {
        var r = new AWSS3Repository<MemoryBlob>(s3client, bucketName);
        var blobs = new List<MemoryBlob>
        {
          new MemoryBlob("AWSS3RepositoryTest.Find1.ext", "payload"),
          new MemoryBlob("folder1/AWSS3RepositoryTest.Find2.ext", "payload"),
          new MemoryBlob("folder1/AWSS3RepositoryTest.Find3.ext", "payload")
        };
        r.CreateMany(blobs);
        if (!useMockContainer)
        {
          Assert.Equal(3, r.Find().Count());
          Assert.Equal(2, r.Find("folder1/").Count());
        }
        r.DeleteMany(blobs);
      }
    }

    [Fact]
    public void GetById()
    {
      using (var s3client = GetAWSS3Client())
      {
        var r = new AWSS3Repository<MemoryBlob>(s3client, bucketName);
        var id = "AWSS3RepositoryTest.GetById.ext";
        var blob = new MemoryBlob(id, "payload");
        r.Create(blob);
        var result = r.GetById(id);
        Assert.NotNull(result);
        r.Delete(result);
      }
    }

    [Fact]
    public void GetById_Not_Found()
    {
      using (var s3client = GetAWSS3Client())
      {
        var r = new AWSS3Repository<MemoryBlob>(s3client, bucketName);
        var id = "AWSS3RepositoryTest.GetById_Not_Found.ext";
        var result = r.GetById(id);
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
