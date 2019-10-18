using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using System.Threading;
using System.IO;
using Xunit;
using RepositoryFramework.Interfaces;
using System.Text;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Services.AppAuthentication;

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

    /// <summary>
    /// Specify environment variable that contains Azure Storage URI
    /// </summary>
    private const string azureStorageAccountNameEnvironmentVariable = "Azure.Storage.AccountName";

    [Fact]
    public void Upload_And_Download_File()
    {
      var uploadFolder = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "Upload");
      Directory.CreateDirectory(uploadFolder);
      using (var file = File.CreateText(Path.Combine(uploadFolder, "file1.txt")))
      {
        file.WriteLine("payload");
      }

      var blobRepository = new AzureBlobRepository(GetCloudBlobContainer());
      var blob = new BlobInfo("cloudFolder/file1.ext");
      using (var uploadStream = new FileStream(Path.Combine(uploadFolder, "file1.txt"), FileMode.Open))
      {
        blobRepository.Upload(blob, uploadStream);
      }

      var downloadFolder = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "Download");
      Directory.CreateDirectory(downloadFolder);

      using (var uploadStream = new FileStream(Path.Combine(downloadFolder, "file1.txt"), FileMode.Create))
      {
        blobRepository.Download(blob, uploadStream);
      }

      if (!useMockContainer)
      {
        Assert.Equal(
          new FileInfo(Path.Combine(uploadFolder, "file1.txt")).Length, 
          new FileInfo(Path.Combine(downloadFolder, "file1.txt")).Length);
        Assert.True(
          File.ReadAllBytes(Path.Combine(uploadFolder, "file1.txt"))
          .SequenceEqual(File.ReadAllBytes(Path.Combine(downloadFolder, "file1.txt"))));
      }

      blobRepository.Delete(blob);
      File.Delete(Path.Combine(uploadFolder, "file1.txt"));
      File.Delete(Path.Combine(downloadFolder, "file1.txt"));
    }


    [Fact]
    public void Upload_And_Download()
    {
      var r = new AzureBlobRepository(GetCloudBlobContainer());
      var blob = new BlobInfo($"AzureBlobRepositoryTest/Upload_And_Download/{Guid.NewGuid()}");
      var payload = "payload";
      Upload(blob, payload, r);
      var result = Download(blob.Id, r);

      if (!useMockContainer)
      {
        Assert.Equal(payload, result);
      }

      r.Delete(blob);
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
      var r = new AzureBlobRepository(GetCloudBlobContainer());
      var blobs = new List<BlobInfo>
      {
        new BlobInfo("AzureBlobRepositoryTest.Find/file1.ext"),
        new BlobInfo("AzureBlobRepositoryTest.Find/folder1/file2.ext"),
        new BlobInfo("AzureBlobRepositoryTest.Find/folder1/file3.ext")
      };
      Upload(blobs[0], "payload1", r);
      Upload(blobs[1], "payload2", r);
      Upload(blobs[2], "payload3", r);
      if (!useMockContainer) 
      {
        Assert.Equal(3, r.Find("AzureBlobRepositoryTest.Find/").Count());
        Assert.Equal(2, r.Find("AzureBlobRepositoryTest.Find/folder1/").Count());
      }
      r.DeleteMany(blobs);
    }

    [Fact]
    public void GetById_Not_Found()
    {
      var r = new AzureBlobRepository(GetCloudBlobContainer());
      var id = "AzureBlobRepositoryTest.GetById_Not_Found.ext";
      var result = r.GetById(id);
      if (!useMockContainer)
      {
        Assert.Null(result);
      }
    }

    private CloudBlobContainer GetCloudBlobContainer()
    {
      if (useMockContainer)
      {
        return GetCloudBlobContainerMock().Object;
      }

      var connectionString = Environment.GetEnvironmentVariable(azureStorageConnectionEnvironmentVariable);
      if (!string.IsNullOrWhiteSpace(connectionString))
      {
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        return blobClient.GetContainerReference("data");
      }

      var accountName = Environment.GetEnvironmentVariable(azureStorageAccountNameEnvironmentVariable);
      if (!string.IsNullOrWhiteSpace(accountName))
      {
        var tokenProvider = new AzureServiceTokenProvider();
        var token = tokenProvider.GetAccessTokenAsync($"https://{accountName}.core.windows.net").GetAwaiter().GetResult();
        var tokenCred = new TokenCredential(token);
        var creds = new StorageCredentials(tokenCred);

        var cloudStorageAccount = new CloudStorageAccount(storageCredentials: creds, accountName: accountName, endpointSuffix: "core.windows.net", useHttps: true);
        var blobClient = cloudStorageAccount.CreateCloudBlobClient();
        return blobClient.GetContainerReference("data");
      }

      return null;
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
