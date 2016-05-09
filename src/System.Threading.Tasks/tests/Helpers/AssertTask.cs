// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    internal static class AssertTask
    {
        /// <summary>
        /// Verify that the given task is complete
        /// </summary>
        /// <param name="task">The task to check</param>
        internal static void Completed(Task task)
        {
            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
            Assert.Null(task.Exception);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        }

        /// <summary>
        /// Verify that the given task is complete
        /// </summary>
        /// <param name="task">The task to check</param>
        /// <param name="result">The expected result of the task</param>
        internal static void Completed<T>(Task<T> task, T result)
        {
            Completed(task);
            Assert.Equal(result, task.Result);
        }

        /// <summary>
        /// Check that the given task is canceled.
        /// </summary>
        /// <param name="task">The task to check</param>
        /// <param name="token">The token the task was canceled with</param>
        internal static void Canceled(Task task, CancellationToken token)
        {
            Assert.True(task.IsCompleted);
            Assert.True(task.IsCanceled);
            Assert.False(task.IsFaulted);
            Assert.Null(task.Exception);
            Assert.Equal(TaskStatus.Canceled, task.Status);

            AggregateException ae = Assert.Throws<AggregateException>(() => task.Wait());
            TaskCanceledException tce = Assert.IsType<TaskCanceledException>(ae.InnerException);
            Assert.Equal(token, tce.CancellationToken);
        }

        /// <summary>
        /// Check that the given task is faulted.
        /// </summary>
        /// <typeparam name="TException">The type of the exception</typeparam>
        /// <param name="task">The task to check</param>
        internal static void Faulted<TException>(Task task) where TException : Exception
        {
            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.True(task.IsFaulted);
            Assert.NotNull(task.Exception);
            Assert.Equal(TaskStatus.Faulted, task.Status);

            AssertThrows.Wrapped<TException>(() => task.Wait());
        }

        /// <summary>
        /// Asserts that two non-generic tasks are logically equal with regards to completion status.
        /// </summary>
        /// <param name="expected">The expected task.</param>
        /// <param name="actual">The actual result task.</param>
        internal static void Equal(Task expected, Task actual)
        {
            Assert.NotNull(actual);
            WaitNoThrow(actual);

            Assert.Equal(expected.Status, actual.Status);
            switch (expected.Status)
            {
                case TaskStatus.Faulted:
                    Assert.Equal((IEnumerable<Exception>)expected.Exception.InnerExceptions, actual.Exception.InnerExceptions);
                    break;

                case TaskStatus.Canceled:
                    Assert.Equal(GetCanceledTaskToken(expected), GetCanceledTaskToken(actual));
                    break;
            }
        }

        /// <summary>
        /// Asserts that two non-generic tasks are logically equal with regards to completion status.
        ///</summary>
        /// <typeparam name="T">The expected type of the result.</typeparam>
        /// <param name="expected">The expected task.</param>
        /// <param name="actual">The actual result task.</param>
        private static void Equal<T>(Task<T> expected, Task<T> actual)
        {
            Equal((Task)expected, actual);
            if (expected.Status == TaskStatus.RanToCompletion)
            {
                if (typeof(T).GetTypeInfo().IsValueType)
                    Assert.Equal(expected.Result, actual.Result);
                else
                    Assert.Same(expected.Result, actual.Result);
            }
        }

        /// <summary>Waits for a task to complete without throwing any exceptions.</summary>
        private static void WaitNoThrow(Task task)
        {
            ((IAsyncResult)task).AsyncWaitHandle.WaitOne();
        }

        /// <summary>Extracts the CancellationToken associated with a task.</summary>
        private static CancellationToken GetCanceledTaskToken(Task task)
        {
            Assert.True(task.IsCanceled);
            TaskAwaiter awaiter = task.GetAwaiter();
            var oce = Assert.Throws<OperationCanceledException>(() => awaiter.GetResult());
            return oce.CancellationToken;
        }
    }
}
