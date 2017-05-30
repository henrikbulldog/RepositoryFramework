using System;
using System.Linq;
using MongoDB.Driver;
using Xunit;
using MongoDB.Bson.Serialization.IdGenerators;
using RepositoryFramework.Interfaces;
using System.Collections.Generic;

namespace RepositoryFramework.MongoDB.Test
{
  public class MongoDBRepositoryTest : IClassFixture<MongoDBFixture>
  {
    private MongoDBFixture mongoDBFixture;

    public MongoDBRepositoryTest(MongoDBFixture mongoDBFixture)
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
    public void GetById()
    {
      var getMe = TestDocument.DummyData1();
      var mongoDBRepository = CreateMongoDBRepository();
      mongoDBRepository.Create(getMe);
      Assert.Equal(getMe.IntTest, mongoDBRepository.GetById(getMe.TestDocumentId).IntTest);
    }

    [Fact]
    public void Find()
    {
      var mongoDBRepository = CreateMongoDBRepository();
      var IEnumerable = mongoDBRepository
        .ClearPaging()
        .Find();
      Assert.True(IEnumerable.Count() > 0);
    }

    [Fact]
    public void FindWhere()
    {
      var mongoDBRepository = CreateMongoDBRepository();
      var result = mongoDBRepository
        .ClearPaging()
        .Find();
      Assert.True(result.Count() > 0);

      var filtered = mongoDBRepository
        .Find(doc => doc.TestDocumentId == result.First().TestDocumentId);
      Assert.Equal(1, filtered.Count());
    }

    [Fact]
    public void FindFilter()
    {
      var mongoDBRepository = CreateMongoDBRepository();
      var result = mongoDBRepository
        .ClearPaging()
        .Find();
      Assert.True(result.Count() > 0);

      var s = @"{ _id: """ + result.First().TestDocumentId + @""" }";
      var filtered = mongoDBRepository
        .Find(s);
      Assert.True(1 == filtered.Count(), s);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SortBy(bool descendingOrder, bool useExpression)
    {
      // Arrange
      ISortableRepository<TestDocument> mongoDBRepository = CreateMongoDBRepository();

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

      var docs = mongoDBRepository.Find().ToList();

      var sortedproducts = descendingOrder
        ? docs.OrderByDescending(p => p.StringTest)
        : docs.OrderBy(p => p.StringTest);

      // Assert
      Assert.NotNull(docs);
      Assert.Equal(docs, sortedproducts);

      // Act
      docs = mongoDBRepository
        .ClearSorting()
        .Find().ToList();

      // Assert
      Assert.NotNull(docs);
      Assert.NotEqual(docs, sortedproducts);
    }

    [Theory]
    [InlineData(1, 20, 30, 20)]
    [InlineData(2, 20, 30, 10)]
    [InlineData(3, 20, 30, 0)]
    public void Page(int page, int pageSize, int totalRows, int expectedRows)
    {
      // Arrange
      IPageableRepository<TestDocument> mongoDBRepository = CreateMongoDBRepository();

      // Act
      var pageItems = mongoDBRepository
        .Page(page, pageSize)
        .Find();

      // Assert
      Assert.NotNull(pageItems);
      Assert.Equal(expectedRows, pageItems.Count());

      // Act
      pageItems = mongoDBRepository
        .ClearPaging()
        .Find();

      // Assert
      Assert.NotNull(pageItems);
      Assert.Equal(totalRows, pageItems.Count());
    }

    [Fact]
    public void PageAndSort()
    {
      var mongoDBRepository = CreateMongoDBRepository();
      var IEnumerable = mongoDBRepository
        .Page(1, 2)
        .SortBy(doc => doc.StringTest)
        .Find();
      Assert.Equal(2, IEnumerable.Count());
    }

    [Fact]
    public void PageAndSortAndAsQueryable()
    {
      var mongoDBRepository = CreateMongoDBRepository();
      var IEnumerable = mongoDBRepository
        .Page(1, 2)
        .SortBy(doc => doc.StringTest)
        .AsQueryable();
      Assert.Equal(2, IEnumerable.Count());
    }

    [Fact]
    public void AsQueryable()
    {
      var mongoDBRepository = CreateMongoDBRepository();
      var IEnumerable = mongoDBRepository
        .AsQueryable()
        .Where(doc => doc.IntTest > 1)
        .OrderBy(doc => doc.StringTest)
        .Take(2);
      Assert.Equal(2, IEnumerable.Count());
    }

    [Fact]
    public void Delete()
    {
      var mongoDBRepository = CreateMongoDBRepository();
      var deleteMe = TestDocument.DummyData1();
      mongoDBRepository.Create(deleteMe);
      Assert.NotNull(mongoDBRepository.GetById(deleteMe.TestDocumentId));

      mongoDBRepository.Delete(deleteMe);
      Assert.Null(mongoDBRepository.GetById(deleteMe.TestDocumentId));
    }

    [Fact]
    public void Update()
    {
      var mongoDBRepository = CreateMongoDBRepository();
      var updateMe = TestDocument.DummyData1();
      mongoDBRepository.Create(updateMe);
      var intTest = updateMe.IntTest + 1;
      updateMe.IntTest = intTest;
      mongoDBRepository.Update(updateMe);

      Assert.Equal(intTest, mongoDBRepository.GetById(updateMe.TestDocumentId).IntTest);
    }
  }
}
