using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryFramework.Interfaces
{
  /// <summary>
  /// Extensions for <see cref="Task"/>
  /// </summary>
  public static class TaskExtensions
  {
    /// <summary>
    /// Wait for the task to complete and rethrow exceptions thrown in another thread
    /// </summary>
    /// <param name="instance">Current instance</param>
    public static void WaitSync(this Task instance)
    {
      Exception exceptionInThread = null;
      try
      {
        instance.Wait();
      }
      catch (AggregateException aggregateException)
      {
        aggregateException.Handle((exc) =>
        {
          exceptionInThread = exc;
          return true;
        });
      }

      if (exceptionInThread != null)
      {
        throw exceptionInThread;
      }
    }
  }
}
