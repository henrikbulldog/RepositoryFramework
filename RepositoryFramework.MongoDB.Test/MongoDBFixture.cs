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
      DatabaseName = Guid.NewGuid().ToString();
      runner = MongoDbRunner.Start();
      Client = new MongoClient(runner.ConnectionString);

    }

    private MongoDbRunner runner;

    public string DatabaseName { get; private set; }

    public MongoClient Client { get; private set; }

    public IMongoDatabase Database
    {
      get
      {
        return Client.GetDatabase(DatabaseName);
      }
    }

    public void Dispose()
    {
      Client.DropDatabase(DatabaseName);
    }
  }
}
