using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryFramework.MongoDB.Test
{
  public class TestDocument
  {
    public string TestDocumentId { get; set; }

    public string StringTest { get; set; }

    public int IntTest { get; set; }

    public DateTime? DateTest { get; set; }

    public List<string> ListTest { get; set; }

    public static TestDocument DummyData1()
    {
      return new TestDocument
      {
        StringTest = "A",
        IntTest = 1,
        DateTest = new DateTime(1984, 09, 30, 6, 6, 6, 171, DateTimeKind.Utc).ToLocalTime(),
        ListTest = new List<string> { "I", "am", "a", "list", "of", "strings" }
      };
    }

    public static TestDocument DummyData2()
    {
      return new TestDocument
      {
        StringTest = "B",
        IntTest = 2,
      };
    }

    public static TestDocument DummyData3()
    {
      return new TestDocument
      {
        StringTest = "C",
        IntTest = 3,
      };
    }
  }
}
