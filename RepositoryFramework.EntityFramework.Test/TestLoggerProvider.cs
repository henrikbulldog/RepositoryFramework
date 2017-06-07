using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using Xunit.Abstractions;

namespace RepositoryFramework.Test
{
  public class TestLoggerProvider : ILoggerProvider
  {
    public TestLoggerProvider(ILogger logger)
    {
      Logger = logger;
    }

    public ILogger Logger { get; set; }

    public ILogger CreateLogger(string categoryName)
    {
      return Logger;
    }

    public void Dispose()
    {
    }
  }
}
