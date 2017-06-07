using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace RepositoryFramework.Test
{
  public class TestLogger : ILogger
  {
    protected readonly ITestOutputHelper output;

    public TestLogger(ITestOutputHelper output)
    {
      this.output = output;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
      return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
      output.WriteLine(formatter(state, exception));
    }

    public IDisposable BeginScope<TState>(TState state)
    {
      return null;
    }
  }
}
