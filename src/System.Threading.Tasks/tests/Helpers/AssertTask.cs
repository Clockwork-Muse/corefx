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
