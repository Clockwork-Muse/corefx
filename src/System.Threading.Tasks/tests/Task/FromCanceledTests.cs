// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class FromCanceledTests
    {
        /// <summary>
        /// Test cases with possible values to feed to FromCanceled
        /// </summary>
        /// Format is:
        ///  1. String label (used for display on test failures)
        ///  2. CancellationToken
        /// <returns>A row of data</returns>
        public static IEnumerable<object[]> Canceled_Data()
        {
            yield return new object[] { "DefaultSource|Canceled", new CancellationToken(true) };
            CancellationTokenSource canceled = new CancellationTokenSource();
            canceled.Cancel();
            yield return new object[] { "NewSource|Canceled", canceled.Token };
        }

        /// <summary>
        /// Test cases with possible values to feed to FromCanceled
        /// </summary>
        /// Format is:
        ///  1. String label (used for display on test failures)
        ///  2. CancellationToken
        /// <returns>A row of data</returns>
        public static IEnumerable<object[]> NotCanceled_Data()
        {
            yield return new object[] { "DefaultSource|None", CancellationToken.None };
            yield return new object[] { "DefaultSource|NotCanceled", new CancellationToken(false) };
            yield return new object[] { "NewSource|NotCanceled", new CancellationTokenSource().Token };
        }

        [Theory]
        [MemberData("Canceled_Data")]
        public static void FromCanceled_Task(string label, CancellationToken token)
        {
            Task task = Task.FromCanceled(token);
            Validate(task, token);

            Assert.NotEqual(task, Task.FromCanceled<int>(token));
            Assert.NotEqual(task, Task.FromCanceled(token));
        }

        [Theory]
        [MemberData("Canceled_Data")]
        public static void FromCanceled_Future(string label, CancellationToken token)
        {
            Task<int> task = Task.FromCanceled<int>(token);
            Validate(task, token);

            // retrieving result throws immediately (Completed task)
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => { int r = task.Result; });

            Assert.NotEqual(task, Task.FromCanceled<int>(token));
            Assert.NotEqual(task, Task.FromCanceled(token));
        }

        private static void Validate(Task task, CancellationToken token)
        {
            Assert.True(task.IsCompleted);
            Assert.True(task.IsCanceled);
            Assert.False(task.IsFaulted);
            Assert.Null(task.Exception);
            Assert.Equal(TaskStatus.Canceled, task.Status);

            // waiting throws immediately (Completed task)
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => task.Wait());

            Assert.Null(task.AsyncState);
            Assert.Equal(TaskCreationOptions.None, task.CreationOptions);
            Assert.Equal(task, task);
        }

        [Theory]
        [MemberData("NotCanceled_Data")]
        public static void FromCanceled_OutOfRange(string label, CancellationToken token)
        {
            // Tokens must be canceled.
            Assert.Throws<ArgumentOutOfRangeException>(() => { Task.FromCanceled(token); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { Task.FromCanceled<int>(token); });
        }
    }
}
