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
			Assert.True(typeof(Foo).CheckPropertyName("Id", out validatedName));
      Assert.True(typeof(Foo).CheckPropertyName("Next", out validatedName));
    }
    [Fact]
		public void Invalid_Path_Should_Return_False()
    {
			string validatedName;
			Assert.False(typeof(Foo).CheckPropertyName("Donald.Duck", out validatedName));
    }
    [Fact]
    public void Should_Not_Be_Case_Insensitive()
    {
			string validatedName;
			Assert.True(typeof(Foo).CheckPropertyName("id", out validatedName));
			Assert.Equal("Id", validatedName);
			Assert.True(typeof(Foo).CheckPropertyName("neXt", out validatedName));
			Assert.Equal("Next", validatedName);
		}
	}
}
