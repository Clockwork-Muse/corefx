// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Reflection;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class FromResultTests
    {
        [Fact]
        public static void RunFromResult()
        {
            // Test FromResult with value type
            {
                var results = new[] { -1, 0, 1, 1, 42, Int32.MaxValue, Int32.MinValue, 42, -42 }; // includes duplicate values to ensure that tasks from these aren't the same object
                Task<int>[] tasks = new Task<int>[results.Length];
                for (int i = 0; i < results.Length; i++)
                    tasks[i] = Task.FromResult(results[i]);

                // Make sure they've all completed
                for (int i = 0; i < tasks.Length; i++)
                    Assert.True(tasks[i].IsCompleted, "TaskRtTests.RunFromResult:    > FAILED: Task " + i + " should have already completed (value)");

                // Make sure they all completed successfully
                for (int i = 0; i < tasks.Length; i++)
                    Assert.True(tasks[i].Status == TaskStatus.RanToCompletion, "TaskRtTests.RunFromResult:    > FAILED: Task " + i + " should have already completed successfully (value)");

                // Make sure no two are the same instance
                for (int i = 0; i < tasks.Length; i++)
                {
                    for (int j = i + 1; j < tasks.Length; j++)
                    {
                        Assert.False(tasks[i] == tasks[j], "TaskRtTests.RunFromResult:    > FAILED: " + i + " and " + j + " created tasks should not be equal (value)");
                    }
                }

                // Make sure they all have the correct results
                for (int i = 0; i < tasks.Length; i++)
                    Assert.True(tasks[i].Result == results[i], "TaskRtTests.RunFromResult:    > FAILED: Task " + i + " had the result " + tasks[i].Result + " but should have had " + results[i] + " (value)");
            }

            // Test FromResult with reference type
            {
                var results = new[] { new object(), null, new object(), null, new object() }; // includes duplicate values to ensure that tasks from these aren't the same object
                Task<Object>[] tasks = new Task<Object>[results.Length];
                for (int i = 0; i < results.Length; i++)
                    tasks[i] = Task.FromResult(results[i]);

                // Make sure they've all completed
                for (int i = 0; i < tasks.Length; i++)
                    Assert.True(tasks[i].IsCompleted, "TaskRtTests.RunFromResult:    > FAILED: Task " + i + " should have already completed  (ref)");

                // Make sure they all completed successfully
                for (int i = 0; i < tasks.Length; i++)
                    Assert.True(tasks[i].Status == TaskStatus.RanToCompletion, "TaskRtTests.RunFromResult:    > FAILED: Task " + i + " should have already completed successfully (ref)");

                // Make sure no two are the same instance
                for (int i = 0; i < tasks.Length; i++)
                {
                    for (int j = i + 1; j < tasks.Length; j++)
                    {
                        Assert.False(tasks[i] == tasks[j], "TaskRtTests.RunFromResult:    > FAILED: " + i + " and " + j + " created tasks should not be equal (ref)");
                    }
                }

                // Make sure they all have the correct results
                for (int i = 0; i < tasks.Length; i++)
                    Assert.True(tasks[i].Result == results[i], "TaskRtTests.RunFromResult:    > FAILED: Task " + i + " had the wrong result (ref)");
            }

            // Test FromException
            {
                var exceptions = new Exception[] { new InvalidOperationException(), new OperationCanceledException(), new Exception(), new Exception() }; // includes duplicate values to ensure that tasks from these aren't the same object
                var tasks = exceptions.Select(e => Task.FromException<int>(e)).ToArray();

                // Make sure they've all completed
                for (int i = 0; i < tasks.Length; i++)
                    Assert.True(tasks[i].IsCompleted, "Task " + i + " should have already completed");

                // Make sure they all completed with an error
                for (int i = 0; i < tasks.Length; i++)
                    Assert.True(tasks[i].Status == TaskStatus.Faulted, "    > FAILED: Task " + i + " should have already faulted");

                // Make sure no two are the same instance
                for (int i = 0; i < tasks.Length; i++)
                {
                    for (int j = i + 1; j < tasks.Length; j++)
                    {
                        Assert.True(tasks[i] != tasks[j], "    > FAILED: " + i + " and " + j + " created tasks should not be equal");
                    }
                }

                // Make sure they all have the correct exceptions
                for (int i = 0; i < tasks.Length; i++)
                {
                    Assert.NotNull(tasks[i].Exception);
                    Assert.Equal(1, tasks[i].Exception.InnerExceptions.Count);
                    Assert.Equal(exceptions[i], tasks[i].Exception.InnerException);
                }

                // Make sure we handle invalid exceptions correctly
                Assert.Throws<ArgumentNullException>(() => { Task.FromException<int>(null); });

                // Make sure we throw from waiting on a faulted task
                Assert.Throws<AggregateException>(() => { var result = Task.FromException<object>(new InvalidOperationException()).Result; });

                // Make sure faulted tasks are actually faulted.  We have little choice for this test but to use reflection,
                // as the harness will crash by throwing from the unobserved event if a task goes unhandled (everywhere
                // other than here it's a bad thing for an exception to go unobserved)
                var faultedTask = Task.FromException<object>(new InvalidOperationException("uh oh"));
                object holderObject = null;
                FieldInfo isHandledField = null;
                var contingentPropertiesField = typeof(Task).GetField("m_contingentProperties", BindingFlags.NonPublic | BindingFlags.Instance);
                if (contingentPropertiesField != null)
                {
                    var contingentProperties = contingentPropertiesField.GetValue(faultedTask);
                    if (contingentProperties != null)
                    {
                        var exceptionsHolderField = contingentProperties.GetType().GetField("m_exceptionsHolder", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (exceptionsHolderField != null)
                        {
                            holderObject = exceptionsHolderField.GetValue(contingentProperties);
                            if (holderObject != null)
                            {
                                isHandledField = holderObject.GetType().GetField("m_isHandled", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            }
                        }
                    }
                }
                Assert.NotNull(holderObject);
                Assert.NotNull(isHandledField);

                Assert.False((bool)isHandledField.GetValue(holderObject), "Expected FromException task to be unobserved before accessing Exception");
                var ignored = faultedTask.Exception;
                Assert.True((bool)isHandledField.GetValue(holderObject), "Expected FromException task to be observed after accessing Exception");
            }
        }
    }
}
