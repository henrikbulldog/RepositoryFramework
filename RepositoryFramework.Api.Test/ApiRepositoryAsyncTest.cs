using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryFramework.Api.Test
{
  public class ApiRepositoryAsyncTest
  {
    private Configuration configuration =
      new Configuration
      {
        AuthenticationType = AuthenticationType.Anonymous
      };

    [Fact]
    public async Task FindAsync_Parameter()
    {
      var apiRepository = new ApiRepository<Post>(
        configuration,
        "https://jsonplaceholder.typicode.com",
        "posts");
      var result = await apiRepository
        .SetParameter("UserId", 1)
        .FindAsync();
      Assert.True(result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));
      Assert.False(result.Any(p => p.UserId != 1));
    }

    [Fact]
    public async Task FindAsync_SubEntities()
    {
      var apiRepository = new ApiRepository<Comment>(
        configuration,
        "https://jsonplaceholder.typicode.com",
        "posts/{postId}/comments");
      var result = await apiRepository
        .SetParameter("PostId", 1)
        .FindAsync();
      Assert.True(result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));
      Assert.False(result.Any(p => p.PostId != 1));
    }

    [Fact]
    public async Task FindAsync_Infer_Entity_Name_And_Id()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var result = await apiRepository
        .SetParameter("UserId", 1)
        .FindAsync();
      Assert.True(result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));
      Assert.False(result.Any(p => p.UserId != 1));
    }

    [Fact]
    public async Task FindAsync()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var result = await apiRepository.FindAsync();
      Assert.True(result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));
    }

    [Fact]
    public async Task FindAsync_Invalid_Path()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com", "bad_path");
      try
      {
        await apiRepository
          .SetParameter("UserId", 1)
          .FindAsync();
      }
      catch (ApiException exc)
      {
        Assert.Equal("GET", exc.Method);
        Assert.Equal("https://jsonplaceholder.typicode.com", exc.BasePath);
        Assert.Equal("bad_path", exc.Path);
        Assert.Equal(404, exc.ErrorCode);
      }
    }

    [Fact]
    public async Task GetByIdAsync()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var result = await apiRepository.GetByIdAsync("1");
      Assert.NotNull(result);
      Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task UpdateAsync()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var post = new Post
      {
        Id = 1,
        UserId = 1,
        Title = "New title",
        Body = "New body"
      };
      await apiRepository.UpdateAsync(post);
      Assert.Equal(1, post.Id);
    }

    [Fact]
    public async Task DeleteAsync()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var post = new Post
      {
        Id = 1
      };
      await apiRepository.DeleteAsync(post);
    }

    [Fact]
    public async Task DeleteManyAsync()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");

      var posts = new List<Post>
      {
        new Post
        {
          Id = 1,
        },
        new Post
        {
          Id = 2,
        },
      };

      await apiRepository.DeleteManyAsync(posts);
    }

    [Fact]
    public async Task CreateAsync()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var post = new Post
      {
        UserId = 1,
        Title = "New title",
        Body = "New body"
      };
      await apiRepository.CreateAsync(post);
      Assert.NotEqual(1, post.Id);
    }

    [Fact]
    public async Task CreateManyAsync()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
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
      await apiRepository.CreateManyAsync(posts);
      Assert.NotEqual(1, posts[0].Id);
    }
  }
}
