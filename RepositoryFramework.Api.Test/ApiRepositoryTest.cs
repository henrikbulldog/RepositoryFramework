namespace RepositoryFramework.Api.Test
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Newtonsoft.Json;
  using RepositoryFramework.Interfaces;
  using Xunit;

  public class ApiRepositoryTest
  {
    private ApiConfiguration configuration =
      new ApiConfiguration
      {
        AuthenticationType = AuthenticationType.Anonymous
      };

    [Fact]
    public async Task Find_Parameter()
    {
      await Find_Parameter_Test(
        async (r) =>
        {
          return await Task.Run(() => r.Find());
        });
      await Find_Parameter_Test(
        async (r) =>
        {
          return await r.FindAsync();
        });
    }

    protected virtual async Task Find_Parameter_Test(
      Func<IParameterizedRepository<Post>, Task<IEnumerable<Post>>> find)
    {
      // Arrange
      IParameterizedRepository<Post> apiRepository = new ApiRepository<Post>(
        configuration,
        "https://jsonplaceholder.typicode.com",
        "posts")
        .SetParameter("UserId", 1);
      var result = await find(apiRepository);

      // Act
      Assert.True(
        result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));

      // Assert
      Assert.DoesNotContain(result, p => p.UserId != 1);
    }

    [Theory]
    [InlineData(2)]
    public async Task Find_SubEntities(int postId)
    {
      await Find_SubEntities_Test(postId,
        async (r) =>
        {
          return await Task.Run(() => r.Find());
        });
      await Find_SubEntities_Test(postId,
        async (r) =>
        {
          return await r.FindAsync();
        });
    }

    protected virtual async Task Find_SubEntities_Test(int postId,
      Func<IApiRepository<Comment>, Task<IEnumerable<Comment>>> find)
    {
      // Arrange
      IApiRepository<Comment> apiRepository = new ApiRepository<Comment>(
        configuration,
        "https://jsonplaceholder.typicode.com",
        "posts/{postId}/comments")
        .SetParameter("PostId", postId);

      // Act
      var result = await find(apiRepository);

      // Assert
      Assert.True(
        result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));
      Assert.DoesNotContain(result, p => p.PostId != postId);
    }

    [Fact]
    public async Task Find_Infer_Entity_Name_And_Id()
    {
      await Find_Infer_Entity_Name_And_Id_Test(
        async (r) =>
        {
          return await Task.Run(() => r.Find());
        });
      await Find_Infer_Entity_Name_And_Id_Test(
        async (r) =>
        {
          return await r.FindAsync();
        });
    }

    protected virtual async Task Find_Infer_Entity_Name_And_Id_Test(
      Func<IApiRepository<Post>, Task<IEnumerable<Post>>> find)
    {
      // Arrange
      IApiRepository<Post> apiRepository = new ApiRepository<Post>(
        configuration, 
        "https://jsonplaceholder.typicode.com")
        .SetParameter("UserId", 1);

      // Act
      var result = await find(apiRepository);

      // Assert
      Assert.True(
        result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));
      Assert.DoesNotContain(result, p => p.UserId != 1);
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
      Func<IApiRepository<Post>, Task<IEnumerable<Post>>> find)
    {
      // Arrange
      IApiRepository<Post> apiRepository = new ApiRepository<Post>(
        configuration, 
        "https://jsonplaceholder.typicode.com");

      // Act
      var result = await find(apiRepository);

      // Assert
      Assert.True(
        result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));
    }

    [Fact]
    public async Task Find_Invalid_Path()
    {
      await Find_Invalid_Path_Test(
        async (r) =>
        {
          return await Task.Run(() => r.Find());
        });
      await Find_Invalid_Path_Test(
        async (r) =>
        {
          return await r.FindAsync();
        });
    }

    protected virtual async Task Find_Invalid_Path_Test(
      Func<IApiRepository<Post>, Task<IEnumerable<Post>>> find)
    {
      // Arrange 
      IApiRepository<Post> apiRepository =
        new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com", "bad_path")
        .SetParameter("UserId", 1);
      try
      {
        await find(apiRepository);
      }
      catch (ApiException exc)
      {
        Assert.Equal("GET", exc.Method);
        Assert.Equal("https://jsonplaceholder.typicode.com", exc.BasePath);
        Assert.Equal("bad_path", exc.Path);
        Assert.Equal(404, exc.ErrorCode);
      }
      catch(Exception)
      {
        throw;
      }
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
      Func<IApiRepository<Post>, object, Task<Post>> getById)
    {
      // Arrange
      IApiRepository<Post> apiRepository =
        new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");

      // Act
      var result = await getById(apiRepository, "1");

      // Assert
      Assert.NotNull(result);
      Assert.Equal(1, result.Id);
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
      Func<IApiRepository<Post>, Post, Task> update)
    {
      // Arrange
      IApiRepository<Post> apiRepository =
        new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");

      // Act
      var post = new Post
      {
        Id = 1,
        UserId = 1,
        Title = "New title",
        Body = "New body"
      };
      await update(apiRepository, post);

      // Assert
      Assert.Equal(1, post.Id);
      Assert.Equal("New title", post.Title);
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
      Func<IApiRepository<Post>, Post, Task> delete)
    {
      // Arrange
      IApiRepository<Post> apiRepository =
        new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");

      // Act
      var post = await apiRepository.GetByIdAsync(1);
      await delete(apiRepository, post);

      // Assert: https://jsonplaceholder.typicode.com returns OK, but does not delete objects
    }

    [Fact]
    public async Task DeleteMany()
    {
      await DeleteManyTest(
        async (r, list) =>
        {
          await Task.Run(() => r.DeleteMany(list));
        });
      await DeleteManyTest(
        async (r, list) =>
        {
          await r.DeleteManyAsync(list);
        });
    }

    protected virtual async Task DeleteManyTest(
      Func<IApiRepository<Post>, IEnumerable<Post>, Task> deleteMany)
    {
      // Arrange
      IApiRepository<Post> apiRepository =
        new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");

      // Act
      var posts = new List<Post>
      {
        await apiRepository.GetByIdAsync(1),
        await apiRepository.GetByIdAsync(2),
      };
      await deleteMany(apiRepository, posts);

      // Assert: https://jsonplaceholder.typicode.com returns OK, but does not delete objects
    }

    [Fact]
    public async Task Create()
    {
      await CreateTest(
        async (r, e) =>
        {
          await Task.Run(() => r.Create(e));
        });
      await CreateTest(
        async (r, e) =>
        {
          await r.CreateAsync(e);
        });
    }

    protected virtual async Task CreateTest(
      Func<IApiRepository<Post>, Post, Task> create)
    {
      // Arrange
      IApiRepository<Post> apiRepository =
        new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");

      // Act
      var post = new Post
      {
        UserId = 1,
        Title = "New title",
        Body = "New body"
      };
      await create(apiRepository, post);

      // Assert
      Assert.NotEqual(1, post.Id);
    }

    [Fact]
    public async Task CreateMany()
    {
      await CreateManyTest(
        async (r, list) =>
        {
          await Task.Run(() => r.CreateMany(list));
        });
      await CreateManyTest(
        async (r, list) =>
        {
          await r.CreateManyAsync(list);
        });
    }

    protected virtual async Task CreateManyTest(
      Func<IApiRepository<Post>, IEnumerable<Post>, Task> createMany)
    {
      // Arrange
      IApiRepository<Post> apiRepository =
        new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");

      // Act
      var posts = new List<Post>
      {
        new Post
        {
          UserId = 1,
          Title = "New title 1",
          Body = "New body 1"
        },
        new Post
        {
          UserId = 1,
          Title = "New title 2",
          Body = "New body 2"
        },
      };
      await createMany(apiRepository, posts);

      // Assert
      Assert.NotEqual(1, posts[0].Id);
      Assert.NotEqual(1, posts[2].Id);
    }
  }
}
