using Xunit;

namespace RepositoryFramework.Test
{
  public class GetPropertyNameTest
  {
    private class Foo
    {
      public Foo() { }
      public int Id { get; set; }
      public Foo Next { get; set; }
    }

    [Fact]
    public void Valid_Path_Should_Return_True()
    {
      string validatedName;
      Assert.True(typeof(Foo).TryCheckPropertyName("Id", out validatedName));
      Assert.True(typeof(Foo).TryCheckPropertyName("Next", out validatedName));
    }
    [Fact]
    public void Invalid_Path_Should_Return_False()
    {
      string validatedName;
      Assert.False(typeof(Foo).TryCheckPropertyName("Donald.Duck", out validatedName));
    }
    [Fact]
    public void Should_Not_Be_Case_Insensitive()
    {
      string validatedName;
      Assert.True(typeof(Foo).TryCheckPropertyName("id", out validatedName));
      Assert.Equal("Id", validatedName);
      Assert.True(typeof(Foo).TryCheckPropertyName("neXt", out validatedName));
      Assert.Equal("Next", validatedName);
    }
  }
}
