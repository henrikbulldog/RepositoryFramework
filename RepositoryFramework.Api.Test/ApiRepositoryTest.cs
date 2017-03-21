using Newtonsoft.Json;
using System.Linq;
using Xunit;

namespace RepositoryFramework.Api.Test
{
  public class ApiRepositoryTest
  {
    private Configuration configuration =
      new Configuration
      {
        AuthenticationType = AuthenticationType.Anonymous
      };

    [Fact]
    public void Find_Filter()
    {
      var apiRepository = new ApiFilterRepository<Post, UserIdFilter>(
        configuration,
        "https://jsonplaceholder.typicode.com",
        "posts",
        (post => post.Id));
      var result = apiRepository.Find(new UserIdFilter { UserId = 1 });
      Assert.True(result.TotalCount > 0,
        JsonConvert.SerializeObject(result.Items, Formatting.Indented));
      Assert.False(result.Items.Any(p => p.UserId != 1));
    }

    [Fact]
    public void Find_Infer_Entity_Name_And_Id()
    {
      var apiRepository = new ApiFilterRepository<Post, UserIdFilter>(configuration, "https://jsonplaceholder.typicode.com");
      var result = apiRepository.Find(new UserIdFilter { UserId = 1 });
      Assert.True(result.TotalCount > 0,
        JsonConvert.SerializeObject(result.Items, Formatting.Indented));
      Assert.False(result.Items.Any(p => p.UserId != 1));
    }

    [Fact]
    public void Find_No_Filter()
    {
      var apiRepository = new ApiRepository<Post>(configuration, "https://jsonplaceholder.typicode.com");
      var result = apiRepository.Find();
      Assert.True(result.TotalCount > 0,
        JsonConvert.SerializeObject(result.Items, Formatting.Indented));
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
  }
}
