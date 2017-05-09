using System;
using System.Linq;
using MongoDB.Driver;
using Xunit;
using MongoDB.Bson.Serialization.IdGenerators;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.MongoDB.Test
{
  public class MongoRepositoryTest : IClassFixture<MongoDBFixture>
  {
    private MongoDBFixture mongoDBFixture;
    private IMongoRepository<TestDocument> mongoRepository;

    public MongoRepositoryTest(MongoDBFixture mongoDBFixture)
    {
      this.mongoDBFixture = mongoDBFixture;
      mongoRepository = new MongoRepository<TestDocument>(mongoDBFixture.Database, d =>
      {
        d.AutoMap();
        d.MapIdMember(c => c.TestDocumentId)
          .SetIdGenerator(StringObjectIdGenerator.Instance);
      });
      mongoRepository.Create(TestDocument.DummyData1());
      mongoRepository.Create(TestDocument.DummyData2());
      mongoRepository.Create(TestDocument.DummyData3());
    }

    [Fact]
    public void GetById()
    {
      var getMe = TestDocument.DummyData1();
      mongoRepository.Create(getMe);
      Assert.Equal(getMe.IntTest, mongoRepository.GetById(getMe.TestDocumentId).IntTest);
    }

    [Fact]
    public void Find()
    {
      var queryResult = mongoRepository.Find();
      Assert.True(queryResult.TotalCount > 0);
    }

    [Fact]
    public void FindAndFilter()
    {
      var queryResult = mongoRepository.Find(
        d => d.StringTest == TestDocument.DummyData2().StringTest
          || d.StringTest == TestDocument.DummyData3().StringTest);
      Assert.True(queryResult.TotalCount > 0);
      Assert.True(queryResult.Items.Any(
        d => d.StringTest == TestDocument.DummyData2().StringTest
            || d.StringTest == TestDocument.DummyData3().StringTest));
    }

    [Fact]
    public void FindAndSort()
    {
      var queryResult = mongoRepository.Find(null, new QueryConstraints<TestDocument>().SortBy(td => td.IntTest));
      Assert.True(queryResult.TotalCount > 0);
      var items = queryResult.Items.ToArray();
      for (int i = 1; i < queryResult.TotalCount; i++)
      {
        Assert.True(items[i - 1].IntTest <= items[i - 1].IntTest);
      }

      queryResult = mongoRepository.Find(null, new QueryConstraints<TestDocument>().SortByDescending(td => td.StringTest));
      Assert.True(queryResult.TotalCount > 0);
      items = queryResult.Items.ToArray();
      for (int i = 1; i < queryResult.TotalCount; i++)
      {
        Assert.True(items[i - 1].IntTest >= items[i - 1].IntTest);
      }
    }

    [Fact]
    public void FindAndPage()
    {
      var queryResult = mongoRepository.Find(null, new QueryConstraints<TestDocument>().Page(1, 2));
      Assert.Equal(2, queryResult.TotalCount);
    }

    [Fact]
    public void Delete()
    {
      var deleteMe = TestDocument.DummyData1();
      mongoRepository.Create(deleteMe);
      Assert.NotNull(mongoRepository.GetById(deleteMe.TestDocumentId));

      mongoRepository.Delete(deleteMe);
      Assert.Null(mongoRepository.GetById(deleteMe.TestDocumentId));
    }

    [Fact]
    public void Update()
    {
      var updateMe = TestDocument.DummyData1();
      mongoRepository.Create(updateMe);
      var intTest = updateMe.IntTest + 1;
      updateMe.IntTest = intTest;
      mongoRepository.Update(updateMe);

      Assert.Equal(intTest, mongoRepository.GetById(updateMe.TestDocumentId).IntTest);
    }
  }
}
