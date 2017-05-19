using System;
using System.Linq;
using MongoDB.Driver;
using Xunit;
using MongoDB.Bson.Serialization.IdGenerators;
using RepositoryFramework.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryFramework.MongoDB.Test
{
  public class MongoDBRepositoryAsyncTest : IClassFixture<MongoDBFixture>
  {
    private MongoDBFixture mongoDBFixture;

    public MongoDBRepositoryAsyncTest(MongoDBFixture mongoDBFixture)
    {
      this.mongoDBFixture = mongoDBFixture;
      var mongoDBRepository = CreateMongoDBRepository();

      if (mongoDBRepository.Find().Count() == 0)
      {
        for (var i = 0; i < 10; i++)
        {
          mongoDBRepository.Create(TestDocument.DummyData1());
          mongoDBRepository.Create(TestDocument.DummyData2());
          mongoDBRepository.Create(TestDocument.DummyData3());
        }
      }
    }

    private IMongoDBRepository<TestDocument> CreateMongoDBRepository()
    {
      return new MongoDBRepository<TestDocument>(mongoDBFixture.Database, d =>
      {
        d.AutoMap();
        d.MapIdMember(c => c.TestDocumentId)
          .SetIdGenerator(StringObjectIdGenerator.Instance);
      });
    }

    [Fact]
    public async Task GetById()
    {
      var getMe = TestDocument.DummyData1();
      var mongoDBRepository = CreateMongoDBRepository();
      await mongoDBRepository.CreateAsync(getMe);
      Assert.Equal(getMe.IntTest, mongoDBRepository.GetById(getMe.TestDocumentId).IntTest);
    }

    [Fact]
    public async Task Find()
    {
      var mongoDBRepository = CreateMongoDBRepository();
      var IEnumerable = await mongoDBRepository
        .ClearPaging()
        .FindAsync();
      Assert.True(IEnumerable.Count() > 0);
    }

    [Fact]
    public async Task FindWhere()
    {
      var mongoDBRepository = CreateMongoDBRepository();
      var result = mongoDBRepository
        .ClearPaging()
        .Find();
      Assert.True(result.Count() > 0);

      var filtered = await mongoDBRepository
        .FindAsync(doc => doc.TestDocumentId == result.First().TestDocumentId);
      Assert.Equal(1, filtered.Count());
    }

    [Fact]
    public async Task FindFilter()
    {
      var mongoDBRepository = CreateMongoDBRepository();
      var result = await mongoDBRepository
        .ClearPaging()
        .FindAsync();
      Assert.True(result.Count() > 0);

      var s = @"{ _id: """ + result.First().TestDocumentId + @""" }";
      var filtered = await mongoDBRepository
        .FindAsync(s);
      Assert.True(1 == filtered.Count(), s);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task SortBy(bool descendingOrder, bool useExpression)
    {
      // Arrange
      var mongoDBRepository = CreateMongoDBRepository();

      // Act
      if (descendingOrder)
      {
        if (useExpression)
        {
          mongoDBRepository.SortByDescending("StringTest");
        }
        else
        {
          mongoDBRepository.SortByDescending(doc => doc.StringTest);
        }
      }
      else
      if (useExpression)
      {
        mongoDBRepository.SortBy("StringTest");
      }
      else
      {
        mongoDBRepository.SortBy(doc => doc.StringTest);
      }

      var docs = await mongoDBRepository.FindAsync();

      var sortedproducts = descendingOrder
        ? docs.OrderByDescending(p => p.StringTest)
        : docs.OrderBy(p => p.StringTest);

      // Assert
      Assert.NotNull(docs);
      Assert.Equal(docs, sortedproducts);

      // Act
      docs = await mongoDBRepository
        .ClearSorting()
        .FindAsync();

      // Assert
      Assert.NotNull(docs);
      Assert.NotEqual(docs, sortedproducts);
    }

    [Theory]
    [InlineData(1, 20, 30, 20)]
    [InlineData(2, 20, 30, 10)]
    [InlineData(3, 20, 30, 0)]
    public async Task Page(int page, int pageSize, int totalRows, int expectedRows)
    {
      // Arrange
      var mongoDBRepository = CreateMongoDBRepository();

      // Act
      var pageItems = mongoDBRepository
        .Page(page, pageSize)
        .Find();

      // Assert
      Assert.NotNull(pageItems);
      Assert.Equal(expectedRows, pageItems.Count());

      // Act
      pageItems = await mongoDBRepository
        .ClearPaging()
        .FindAsync();

      // Assert
      Assert.NotNull(pageItems);
      Assert.Equal(totalRows, pageItems.Count());
    }

    [Fact]
    public async Task PageAndSort()
    {
      var mongoDBRepository = CreateMongoDBRepository();
      var IEnumerable = await mongoDBRepository
        .SortBy(doc => doc.StringTest)
        .Page(1, 2)
        .FindAsync();
      Assert.Equal(2, IEnumerable.Count());
    }

    [Fact]
    public async Task Delete()
    {
      var mongoDBRepository = CreateMongoDBRepository();
      var deleteMe = TestDocument.DummyData1();
      await mongoDBRepository.CreateAsync(deleteMe);
      Assert.NotNull(mongoDBRepository.GetById(deleteMe.TestDocumentId));

      await mongoDBRepository.DeleteAsync(deleteMe);
      Assert.Null(mongoDBRepository.GetById(deleteMe.TestDocumentId));
    }

    [Fact]
    public async Task Update()
    {
      var mongoDBRepository = CreateMongoDBRepository();
      var updateMe = TestDocument.DummyData1();
      await mongoDBRepository.CreateAsync(updateMe);
      var intTest = updateMe.IntTest + 1;
      updateMe.IntTest = intTest;
      await mongoDBRepository.UpdateAsync(updateMe);

      Assert.Equal(intTest, mongoDBRepository.GetById(updateMe.TestDocumentId).IntTest);
    }
  }
}
