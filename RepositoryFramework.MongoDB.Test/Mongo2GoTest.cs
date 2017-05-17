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
    private MongoDBFixture mongoDBFixture;
    private string collectionName = "TestCollection";

    public Mongo2GoTest(MongoDBFixture mongoDBFixture)
    {
      this.mongoDBFixture = mongoDBFixture;
      if (!BsonClassMap.IsClassMapRegistered(typeof(TestDocument)))
      {
        try
        {
          BsonClassMap.RegisterClassMap<TestDocument>(
            map =>
            {
              map.AutoMap();
              map.MapIdMember(c => c.TestDocumentId)
                .SetIdGenerator(StringObjectIdGenerator.Instance);
            });
        }
        catch
        {
        }
      }
    }

    public static IList<T> ReadBsonFile<T>(string fileName)
    {
      string[] content = File.ReadAllLines(fileName);
      return content.Select(s => BsonSerializer.Deserialize<T>(s)).ToList();
    }

    [Fact]
    public void SaveAndRetrieve()
    {
      var collection = mongoDBFixture.Database
        .GetCollection<TestDocument>(collectionName);

      List<TestDocument> IEnumerable;

      collection.InsertOne(TestDocument.DummyData1());
      collection.InsertOne(TestDocument.DummyData2());
      collection.InsertOne(TestDocument.DummyData3());

      IEnumerable = (from c in collection.AsQueryable()
                     where c.StringTest == TestDocument.DummyData2().StringTest || c.StringTest == TestDocument.DummyData3().StringTest
                     select c).ToList();

      Assert.Equal(2, IEnumerable.Count());
    }
  }
}
