using Mongo2Go;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using System.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using Xunit;
using MongoDB.Bson.Serialization.IdGenerators;
using RepositoryFramework.Interfaces;

namespace RepositoryFramework.MongoDB.Test
{
  public class RepositoryTest : IClassFixture<MongoDBFixture>
  {
    public RepositoryTest(MongoDBFixture mongoDBFixture)
    {
      this.mongoDBFixture = mongoDBFixture;
    }

    private MongoDBFixture mongoDBFixture;

    [Fact]
    public void SaveAndRetrieve()
    {
      IQueryResult<TestDocument> queryResult;

      var repo = new Repository<TestDocument>(mongoDBFixture.Database, d =>
      {
        d.AutoMap();
        d.MapIdMember(c => c.TestDocumentId)
          .SetIdGenerator(StringObjectIdGenerator.Instance);
      });

      var d1 = TestDocument.DummyData1();
      var testDocumentIdBeforeCreate = d1.TestDocumentId;

      repo.Create(d1);
      repo.Create(TestDocument.DummyData2());
      repo.Create(TestDocument.DummyData3());

      queryResult = repo.Find(set => set.Where(c => c.StringTest == TestDocument.DummyData2().StringTest || c.StringTest == TestDocument.DummyData3().StringTest));

      Assert.Null(testDocumentIdBeforeCreate);
      Assert.NotNull(d1.TestDocumentId);
      Assert.Equal(2, queryResult.TotalCount);
    }
  }


}
