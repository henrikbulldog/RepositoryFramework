using System;
using System.Linq;
using MongoDB.Driver;
using Xunit;
using MongoDB.Bson.Serialization.IdGenerators;
using RepositoryFramework.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;

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
    public async Task GetById()
    {
      await GetByIdTest(
        async (r, id) =>
        {
          return await Task.Run(() => r.GetById(id));
        });
      await GetByIdTest(
        async (r, id) =>
        {
          return await r.GetByIdAsync(id);
        });
    }

    protected virtual async Task GetByIdTest(
      Func<IMongoDBRepository<TestDocument>, object, Task<TestDocument>> getById)
    {
      // Arrange
      var mongoDBRepository = CreateMongoDBRepository();

      // Act
      var getMe = TestDocument.DummyData1();
      mongoDBRepository.Create(getMe);

      // Assert
      var actual = await getById(mongoDBRepository, getMe.TestDocumentId);
      Assert.NotNull(actual);
      Assert.Equal(getMe.IntTest, actual.IntTest);
    }

    [Fact]
    public async Task Find()
    {
      await FindTest(
        async (r) =>
        {
          return await Task.Run(() => r.Find());
        });
      await FindTest(
        async (r) =>
        {
          return await r.FindAsync();
        });
    }

    protected virtual async Task FindTest(
      Func<IMongoDBRepository<TestDocument>, Task<IEnumerable<TestDocument>>> find)
    {
      // Arrange
      var mongoDBRepository = CreateMongoDBRepository()
        .ClearPaging();

      // Act
      var IEnumerable = await find(mongoDBRepository);

      // Assert
      Assert.True(IEnumerable.Count() > 0);
    }

    [Fact]
    public async Task FindWhere()
    {
      await FindWhereTest(
        async (r, where) =>
        {
          return await Task.Run(() => r.Find(where));
        });
      await FindWhereTest(
        async (r, where) =>
        {
          return await r.FindAsync(where);
        });
    }

    protected virtual async Task FindWhereTest(
      Func<IMongoDBRepository<TestDocument>, Expression<Func<TestDocument, bool>>, Task<IEnumerable<TestDocument>>> find)
    {
      // Arrange
      var mongoDBRepository = CreateMongoDBRepository();
      var result = mongoDBRepository.Find();

      // Act
      var filtered = await find(mongoDBRepository,
        doc => doc.TestDocumentId == result.First().TestDocumentId);

      // Assert
      Assert.Single(filtered);
    }

    [Fact]
    public async Task FindFilter()
    {
      await FindFilterTest(
        async (r, filter) =>
        {
          return await Task.Run(() => r.Find(filter));
        });
      await FindFilterTest(
        async (r, filter) =>
        {
          return await r.FindAsync(filter);
        });
    }

    protected virtual async Task FindFilterTest(
      Func<IMongoDBRepository<TestDocument>, string, Task<IEnumerable<TestDocument>>> find)
    {
      // Arrange
      var mongoDBRepository = CreateMongoDBRepository();
      var result = await mongoDBRepository.FindAsync();

      // Act
      var s = @"{ _id: """ + result.First().TestDocumentId + @""" }";
      var filtered = await find(mongoDBRepository, s);

      // Assert
      Assert.True(1 == filtered.Count(), s);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task SortBy(bool descendingOrder, bool useExpression)
    {
      await SortByTest(descendingOrder, useExpression,
        async (r) =>
        {
          return await Task.Run(() => r.Find());
        });
      await SortByTest(descendingOrder, useExpression,
        async (r) =>
        {
          return await r.FindAsync();
        });
    }
    protected virtual async Task SortByTest(bool descendingOrder, bool useExpression,
      Func<IMongoDBRepository<TestDocument>, Task<IEnumerable<TestDocument>>> find)
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

      var sortedDocs = await find(mongoDBRepository);
      var sortedproducts = descendingOrder
        ? sortedDocs.OrderByDescending(p => p.StringTest)
        : sortedDocs.OrderBy(p => p.StringTest);
      var unsortedDocs = await find(mongoDBRepository.ClearSorting());

      // Assert
      Assert.NotNull(sortedDocs);
      Assert.Equal(sortedDocs, sortedproducts);
      Assert.NotNull(unsortedDocs);
      Assert.NotEqual(unsortedDocs, sortedproducts);
    }

    [Theory]
    [InlineData(1, 20, 30, 20)]
    [InlineData(2, 20, 30, 10)]
    [InlineData(3, 20, 30, 0)]
    [InlineData(1, 0, 30, 30)]
    public async Task Page(int page, int pageSize, int totalRows, int expectedRows)
    {
      await PageTest(page, pageSize, totalRows, expectedRows,
        async (r) =>
        {
          return await Task.Run(() => r.Find());
        });
      await PageTest(page, pageSize, totalRows, expectedRows,
        async (r) =>
        {
          return await r.FindAsync();
        });
    }

    protected virtual async Task PageTest(int page, int pageSize, int totalRows, int expectedRows,
      Func<IMongoDBRepository<TestDocument>, Task<IEnumerable<TestDocument>>> find)
    {
      // Arrange
      var mongoDBRepository = CreateMongoDBRepository()
        .Page(page, pageSize);

      // Act
      var pageItems = await find(mongoDBRepository);

      // Assert
      Assert.NotNull(pageItems);
      Assert.Equal(expectedRows, pageItems.Count());
      Assert.Equal(totalRows, mongoDBRepository.TotalItems);
      Assert.Equal(pageSize == 0 ? 1 : (totalRows / pageSize) + 1, mongoDBRepository.TotalPages);
      Assert.Equal(page, mongoDBRepository.PageNumber);
      Assert.Equal(page < 2 ? 1 : ((page - 1) * pageSize) + 1, mongoDBRepository.StartIndex);
    }

    [Fact]
    public async Task PageAndSort()
    {
      await PageAndSortTest(
        async (r) =>
        {
          return await Task.Run(() => r.Find());
        });
      await PageAndSortTest(
        async (r) =>
        {
          return await r.FindAsync();
        });
    }

    protected virtual async Task PageAndSortTest(
      Func<IMongoDBRepository<TestDocument>, Task<IEnumerable<TestDocument>>> find)
    {
      // Arrange
      var mongoDBRepository = CreateMongoDBRepository()
        .Page(1, 2)
        .SortBy(doc => doc.StringTest);

      // Act
      var IEnumerable = await find(mongoDBRepository);

      // Assert
      Assert.Equal(2, IEnumerable.Count());
    }

    [Fact]
    public async Task Delete()
    {
      await DeleteTest(
        async (r, e) =>
        {
          await Task.Run(() => r.Delete(e));
        });
      await DeleteTest(
        async (r, e) =>
        {
          await r.DeleteAsync(e);
        });
    }

    protected virtual async Task DeleteTest(
      Func<IMongoDBRepository<TestDocument>, TestDocument, Task> delete)
    {
      // Arrange
      var mongoDBRepository = CreateMongoDBRepository();
      var deleteMe = TestDocument.DummyData1();
      await mongoDBRepository.CreateAsync(deleteMe);

      // Act
      await delete(mongoDBRepository, deleteMe);

      // Assert
      Assert.Null(mongoDBRepository.GetById(deleteMe.TestDocumentId));
    }

    [Fact]
    public async Task Update()
    {
      await UpdateTest(
        async (r, e) =>
        {
          await Task.Run(() => r.Update(e));
        });
      await UpdateTest(
        async (r, e) =>
        {
          await r.UpdateAsync(e);
        });
    }

    protected virtual async Task UpdateTest(
      Func<IMongoDBRepository<TestDocument>, TestDocument, Task> update)
    {
      // Arrange
      var mongoDBRepository = CreateMongoDBRepository();
      var updateMe = TestDocument.DummyData1();
      await mongoDBRepository.CreateAsync(updateMe);
      updateMe.IntTest++;

      // Act
      await update(mongoDBRepository, updateMe);

      // Assert
      var actual = await mongoDBRepository.GetByIdAsync(updateMe.TestDocumentId);
      Assert.Equal(updateMe.IntTest, actual.IntTest);
    }

    [Fact]
    public void PageAndSortAndAsQueryable()
    {
      // Arrange
      var mongoDBRepository = CreateMongoDBRepository();

      // Act
      var IEnumerable = mongoDBRepository
        .Page(1, 2)
        .SortBy(doc => doc.StringTest)
        .AsQueryable();

      // Assert
      Assert.Equal(2, IEnumerable.Count());
    }
    
    [Fact]
    public void AsQueryable()
    {
      // Arrange
      var mongoDBRepository = CreateMongoDBRepository();

      // Act
      var IEnumerable = mongoDBRepository
        .AsQueryable()
        .Where(doc => doc.IntTest > 1)
        .OrderBy(doc => doc.StringTest)
        .Take(2);

      // Assert
      Assert.Equal(2, IEnumerable.Count());
    }
  }
}
