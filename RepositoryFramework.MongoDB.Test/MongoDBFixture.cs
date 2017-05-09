using Mongo2Go;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryFramework.MongoDB.Test
{
  public class MongoDBFixture : IDisposable
  {
    public MongoDBFixture()
    {
      runner = MongoDbRunner.Start(@".\data\");
      Client = new MongoClient(runner.ConnectionString);
      Database = Client.GetDatabase(databaseName);
    }

    private MongoDbRunner runner;

    private readonly string databaseName = Guid.NewGuid().ToString();

    public IMongoDatabase Database { get; private set; }

    public MongoClient Client { get; private set; }


    public void Dispose()
    {
      Client.DropDatabase(databaseName);
      runner.Dispose();
    }
  }
}
