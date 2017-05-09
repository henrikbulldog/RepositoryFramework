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

namespace RepositoryFramework.MongoDB.Test
{
  public class Mongo2GoTest : IClassFixture<MongoDBFixture>
  {
    public Mongo2GoTest(MongoDBFixture mongoDBFixture)
    {
      this.mongoDBFixture = mongoDBFixture;

      if (!BsonClassMap.IsClassMapRegistered(typeof(TestDocument)))
      {
        BsonClassMap.RegisterClassMap<TestDocument>(
      d =>
      {
        d.AutoMap();
        d.MapIdMember(c => c.TestDocumentId)
          .SetIdGenerator(StringObjectIdGenerator.Instance); ;
      });
      }

      collection = mongoDBFixture.Database.GetCollection<TestDocument>(collectionName);
    }

    private MongoDBFixture mongoDBFixture;
    internal static IMongoCollection<TestDocument> collection;
    internal static string collectionName = "TestCollection";

    public static IList<T> ReadBsonFile<T>(string fileName)
    {
      string[] content = File.ReadAllLines(fileName);
      return content.Select(s => BsonSerializer.Deserialize<T>(s)).ToList();
    }

    [Fact]
    public void SaveAndRetrieve()
    {
      List<TestDocument> queryResult;

      collection.InsertOne(TestDocument.DummyData1());
      collection.InsertOne(TestDocument.DummyData2());
      collection.InsertOne(TestDocument.DummyData3());

      queryResult = (from c in collection.AsQueryable()
                     where c.StringTest == TestDocument.DummyData2().StringTest || c.StringTest == TestDocument.DummyData3().StringTest
                     select c).ToList();

      Assert.Equal(2, queryResult.Count());
    }
  }
}
