using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RepositoryFramework.Api.Test
{
  public class ApiRepositoryTest
  {
    private ApiConfiguration configuration =
      new ApiConfiguration
      {
        AuthenticationType = AuthenticationType.Anonymous
      };

    [Fact]
    public void Find_Parameter()
    {
      var apiRepository = new ApiRepository<Post>(
        configuration,
        "https://jsonplaceholder.typicode.com",
        "posts");
      var result = apiRepository
        .SetParameter("UserId", 1)
        .Find();
      Assert.True(result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));
      Assert.False(result.Any(p => p.UserId != 1));
    }

    [Fact]
    public void Find_SubEntities()
    {
      var apiRepository = new ApiRepository<Comment>(
        configuration,
        "https://jsonplaceholder.typicode.com",
        "posts/{postId}/comments");
      var result = apiRepository
        .SetParameter("PostId", 1)
        .Find();
      Assert.True(result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));
      Assert.False(result.Any(p => p.PostId != 1));
    }

    [Fact]
    public void Find_Infer_Entity_Name_And_Id()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var result = apiRepository
        .SetParameter("UserId", 1)
        .Find();
      Assert.True(result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));
      Assert.False(result.Any(p => p.UserId != 1));
    }

    [Fact]
    public void Find()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var result = apiRepository.Find();
      Assert.True(result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));
    }

    [Fact]
    public void Find_Invalid_Path()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com", "bad_path");
      try
      {
        apiRepository
          .SetParameter("UserId", 1)
          .Find();
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
    public void GetById()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var result = apiRepository.GetById("1");
      Assert.NotNull(result);
      Assert.Equal(1, result.Id);
    }

    [Fact]
    public void Update()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var post = new Post
      {
        Id = 1,
        UserId = 1,
        Title = "New title",
        Body = "New body"
      };
      apiRepository.Update(post);
      Assert.Equal(1, post.Id);
    }

    [Fact]
    public void Delete()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var post = new Post
      {
        Id = 1
      };
      apiRepository.Delete(post);
    }

    [Fact]
    public void DeleteMany()
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

      apiRepository.DeleteMany(posts);
    }

    [Fact]
    public void Create()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var post = new Post
      {
        UserId = 1,
        Title = "New title",
        Body = "New body"
      };
      apiRepository.Create(post);
      Assert.NotEqual(1, post.Id);
    }

    [Fact]
    public void CreateMany()
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
      apiRepository.CreateMany(posts);
      Assert.NotEqual(1, posts[0].Id);
    }
  }
}
