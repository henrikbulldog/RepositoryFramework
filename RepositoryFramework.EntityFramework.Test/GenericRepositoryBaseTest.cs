using RepositoryFramework.Interfaces;
using RepositoryFramework.Test.Models;
using System;
using System.Linq.Expressions;
using Xunit;

namespace RepositoryFramework.Test
{
  public class GenericRepositoryBaseTest
  {
    private class Foo
    {
      public Foo() { }
      public int Id { get; set; }
      public Foo Next { get; set; }
    }

    private class GenericRepostoryTest<TEntity> : GenericRepositoryBase<TEntity>
      where TEntity : class
    {
      public bool TryCheckPropertyNameAccessor(string property, out string validatedPropertyName)
      {
        return TryCheckPropertyName(property, out validatedPropertyName);
      }

      public bool TryCheckPropertyPathAccessor(string path, out string validatedPath)
      {
        return TryCheckPropertyPath(path, out validatedPath);
      }
    }

    [Theory]
    [InlineData("Id", true, "Id")]
    [InlineData("Next", true, "Next")]
    [InlineData("Invalid", false, "Invalid")]
    [InlineData("id", true, "Id")]
    [InlineData("neXt", true, "Next")]
    public void TryCheckPropertyName(string propertyName, bool expectedReturn, string expectedValidatedName)
    {
      var genericRepostoryTest = new GenericRepostoryTest<Foo>();
      string validatedName;
      bool actualReturn = genericRepostoryTest.TryCheckPropertyNameAccessor(propertyName, out validatedName);
      Assert.Equal(expectedReturn, actualReturn);
      Assert.Equal(expectedValidatedName, validatedName);
    }

    [Theory]
    [InlineData("Id", true, "Id")]
    [InlineData("neXt.iD", true, "Next.Id")]
    [InlineData("Invalid.Path", false, "Invalid.Path")]
    public void TryCheckPropertyPath_Foo(
      string propertyPath, 
      bool expectedReturn, 
      string expectedValidatedPath)
    {
      var genericRepostoryTest = new GenericRepostoryTest<Foo>();
      string validatedPath;
      bool actualReturn = genericRepostoryTest.TryCheckPropertyPathAccessor(propertyPath, out validatedPath);
      Assert.Equal(expectedReturn, actualReturn);
      Assert.Equal(expectedValidatedPath, validatedPath);
    }

    [Theory]
    [InlineData("products.parts", true, "Products.Parts")]
    public void TryCheckPropertyPath_Category(string propertyPath, bool expectedReturn, string expectedValidatedPath)
    {
      var genericRepostoryTest = new GenericRepostoryTest<Category>();
      string validatedPath;
      bool actualReturn = genericRepostoryTest.TryCheckPropertyPathAccessor(propertyPath, out validatedPath);
      Assert.Equal(expectedReturn, actualReturn);
      Assert.Equal(expectedValidatedPath, validatedPath);
    }
  }
}
