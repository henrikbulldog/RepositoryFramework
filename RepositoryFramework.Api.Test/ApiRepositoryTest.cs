namespace RepositoryFramework.Api.Test
{
  using System.Collections.Generic;
  using System.Linq;
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
    public void Find_Parameter()
    {
      IParameterizedRepository<Post> apiRepository = new ApiRepository<Post>(
        configuration,
        "https://jsonplaceholder.typicode.com",
        "posts");
      var result = apiRepository
        .SetParameter("UserId", 1)
        .Find();
      Assert.True(
        result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));
      Assert.False(result.Any(p => p.UserId != 1));
    }

    [Fact]
    public void Find_SubEntities()
    {
      IApiRepository<Comment> apiRepository = new ApiRepository<Comment>(
        configuration,
        "https://jsonplaceholder.typicode.com",
        "posts/{postId}/comments");
      var result = apiRepository
        .SetParameter("PostId", 1)
        .Find();
      Assert.True(
        result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));
      Assert.False(result.Any(p => p.PostId != 1));
    }

    [Fact]
    public void Find_Infer_Entity_Name_And_Id()
    {
      ApiRepository<Post> apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var result = apiRepository
        .SetParameter("UserId", 1)
        .Find();
      Assert.True(
        result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));
      Assert.False(result.Any(p => p.UserId != 1));
    }

    [Fact]
    public void Find()
    {
      IApiRepository<Post> apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var result = apiRepository.Find();
      Assert.True(
        result.Count() > 0,
        JsonConvert.SerializeObject(result, Formatting.Indented));
    }

    [Fact]
    public void Find_Invalid_Path()
    {
      IApiRepository<Post> apiRepository =
        new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com", "bad_path");
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
      IApiRepository<Post> apiRepository =
        new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var result = apiRepository.GetById("1");
      Assert.NotNull(result);
      Assert.Equal(1, result.Id);
    }

    [Fact]
    public void Update()
    {
      IApiRepository<Post> apiRepository =
        new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
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
      IApiRepository<Post> apiRepository =
        new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var post = new Post
      {
        Id = 1
      };
      apiRepository.Delete(post);
    }

    [Fact]
    public void DeleteMany()
    {
      IApiRepository<Post> apiRepository =
        new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");

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
      IApiRepository<Post> apiRepository =
        new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
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
      IApiRepository<Post> apiRepository =
        new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
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
