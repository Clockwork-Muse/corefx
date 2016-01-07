// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.Threading.Tasks.Tests
{
    internal static class Functions
    {
        public static void AssertThrowsWrapped<T>(Action query)
        {
            AggregateException ae = Assert.Throws<AggregateException>(query);
            Assert.All(ae.InnerExceptions, e => Assert.IsType<T>(e));
        }

        public async static Task<Task> AssertThrowsAsync<T>(Func<Task> query) where T : Exception
        {
            Task t = query();
            await Assert.ThrowsAsync<T>(() => t);
            return t;
        }

        /// <summary>
        /// Simulate workload by spinning for the given time, then returning the given value
        /// </summary>
        /// <typeparam name="T">Type of the given value</typeparam>
        /// <param name="time">How long to spin</param>
        /// <param name="value">The value to return</param>
        /// <returns>Simulated result of work</returns>
        internal static T SpinAndReturn<T>(TimeSpan time, T value)
        {
            SpinWait.SpinUntil(() => false, time);
            return value;
        }

        /// <summary>
        /// Simulate workload by spinning for the given time, then doing the given action
        /// </summary>
        /// <param name="time">How long to spin</param>
        /// <param name="action">The action to perform</param>
        internal static void SpinAndDo(TimeSpan time, Action action)
        {
            SpinWait.SpinUntil(() => false, time);
            action();
        }

        /// <summary>
        /// Verify that the given task is complete
        /// </summary>
        /// <param name="task">The task to check</param>
        internal static void AssertComplete(Task task)
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
        internal static void AssertComplete<T>(Task<T> task, T result)
        {
            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
            Assert.Null(task.Exception);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.Equal(result, task.Result);
        }

        /// <summary>
        /// Check that the given task is canceled.
        /// </summary>
        /// <param name="task">The task to check</param>
        /// <param name="token">The token the task was canceled with</param>
        internal static void AssertCanceled(Task task, CancellationToken token)
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
        internal static void AssertFaulted<TException>(Task task)
        {
            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.True(task.IsFaulted);
            Assert.NotNull(task.Exception);
            Assert.Equal(TaskStatus.Faulted, task.Status);

            AssertThrowsWrapped<TException>(() => task.Wait());
        }
    }
}
