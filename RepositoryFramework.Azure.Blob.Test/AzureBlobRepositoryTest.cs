using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using System.IO;
using Xunit;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.Azure.Blob.Test
{
  public class AzureBlobRepositoryTest
  {
    /// <summary>
    /// Set to false to test Microsoft Azure Storage Service and set connection string in environment variable azureStorageConnectionEnvironmentVariable
    /// </summary>
    private bool useMockContainer = true;

    /// <summary>
    /// Specify environment variable that contains Azure Storage connection string
    /// </summary>
    private const string azureStorageConnectionEnvironmentVariable = "Azure.Storage.Connection";

    [Fact]
    public void Create()
    {
      var r = new AzureBlobRepository<MemoryBlob>(GetCloudBlobContainer());
      var blob = new MemoryBlob("AzureBlobRepositoryTest.Create.ext", "payload");
      r.Create(blob);
      r.Delete(blob);
    }

    [Fact]
    public void CreateMany()
    {
      var r = new AzureBlobRepository<MemoryBlob>(GetCloudBlobContainer());
      var blobs = new List<MemoryBlob>
      {
        new MemoryBlob("AzureBlobRepositoryTest.CreateMany1.ext", "payload"),
        new MemoryBlob("AzureBlobRepositoryTest.CreateMany2.ext", "payload"),
        new MemoryBlob("AzureBlobRepositoryTest.CreateMany3.ext", "payload")
      };
      r.CreateMany(blobs);
      r.DeleteMany(blobs);
    }

    [Fact]
    public void Update()
    {
      var r = new AzureBlobRepository<MemoryBlob>(GetCloudBlobContainer());
      var blob = new MemoryBlob("AzureBlobRepositoryTest.Update.ext", "payload");
      r.Create(blob);
      r.Update(blob);
      r.Delete(blob);
    }

    [Fact]
    public void Find()
    {
      var r = new AzureBlobRepository<MemoryBlob>(GetCloudBlobContainer());
      var blobs = new List<MemoryBlob>
      {
        new MemoryBlob("AzureBlobRepositoryTest.Find1.ext", "payload"),
        new MemoryBlob("folder1/AzureBlobRepositoryTest.Find2.ext", "payload"),
        new MemoryBlob("folder1/AzureBlobRepositoryTest.Find3.ext", "payload")
      };
      r.CreateMany(blobs);
      if (!useMockContainer)
      {
        Assert.Equal(3, r.Find().Count());
        Assert.Equal(2, r.Find("folder1/").Count());
      }
      r.DeleteMany(blobs);
    }

    [Fact]
    public void GetById()
    {
      var r = new AzureBlobRepository<MemoryBlob>(GetCloudBlobContainer());
      var id = "AzureBlobRepositoryTest.GetById.ext";
      var blob = new MemoryBlob(id, "payload");
      r.Create(blob);
      var result = r.GetById(id);
      Assert.NotNull(result);
      r.Delete(result);
    }

    [Fact]
    public void GetById_Not_Found()
    {
      var r = new AzureBlobRepository<MemoryBlob>(GetCloudBlobContainer());
      var id = "AzureBlobRepositoryTest.GetById_Not_Found.ext";
      var result = r.GetById(id);
    }


    private CloudBlobContainer GetCloudBlobContainer()
    {
      if (useMockContainer)
      {
        return GetCloudBlobContainerMock().Object;
      }
      var connectionString = Environment.GetEnvironmentVariable(azureStorageConnectionEnvironmentVariable);
      if (string.IsNullOrWhiteSpace(connectionString))
      {
        throw new Exception($"Environment variable {azureStorageConnectionEnvironmentVariable} must be set");
      }
      CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
      CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
      return blobClient.GetContainerReference("data");
    }

    private Mock<CloudBlobContainer> GetCloudBlobContainerMock()
    {
      var mockBlobUri = new Uri("http://bogus/myaccount/blob");
      var mockBlobContainer = new Mock<CloudBlobContainer>(MockBehavior.Loose, mockBlobUri);
      var mockBlobItem = new Mock<CloudBlockBlob>(MockBehavior.Loose, mockBlobUri);
      mockBlobContainer
        .Setup(c => c.GetBlockBlobReference(It.IsAny<string>()))
        .Returns(mockBlobItem.Object)
        .Verifiable();
      return mockBlobContainer;
    }
  }
}
