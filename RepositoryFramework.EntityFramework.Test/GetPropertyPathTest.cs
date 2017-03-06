using RepositoryFramework.Test.Models;
using Xunit;

namespace RepositoryFramework.Test
{
  public class GetPropertyPathTest
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
		public void Valid_Nested_Path_Should_Return_True()
		{
			string validatedPath;
			Assert.True(typeof(Foo).CheckPropertyPath("Next.Id", out validatedPath));
			Assert.True(typeof(Foo).CheckPropertyPath("Next.Next.Next", out validatedPath));
			Assert.True(typeof(Category).CheckPropertyPath("Products.Parts", out validatedPath));
		}
		[Fact]
		public void Valid_Nested_Path_Should_Be_Case_Insensitive()
		{
			string validatedPath;
			Assert.True(typeof(Foo).CheckPropertyPath("neXt.iD", out validatedPath));
			Assert.Equal("Next.Id", validatedPath);
			Assert.True(typeof(Category).CheckPropertyPath("products.parts", out validatedPath));
			Assert.Equal("Products.Parts", validatedPath);
		}
		[Fact]
    public void Invalid_Path_Should_Return_False()
    {
			string validatedPath;
			Assert.False(typeof(Foo).CheckPropertyPath("Donald.Duck", out validatedPath));
    }
  }
}
