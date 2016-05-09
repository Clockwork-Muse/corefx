// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Threading.Tasks.Tests
{
    internal static class Functions
    {
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
        /// Get the scheduler expected to be visible to tasks started by the factory.
        /// </summary>
        /// If the factory wasn't created with a scheduler, or it was but has the `HideScheduler` flag set,
        /// this method returns the default (threadpool) scheduler.
        /// Otherwise, it returns the scheduler the factory was created with.
        /// <param name="factory">The factory to get the expected scheduler for.</param>
        /// <returns>The scheduler visible to started tasks.</returns>
        internal static TaskScheduler ExpectedScheduler(this TaskFactory factory)
        {
            return (factory.Scheduler == null || factory.CreationOptions.HasFlag(TaskCreationOptions.HideScheduler) ? TaskScheduler.Default : factory.Scheduler);
        }

        /// <summary>
        /// Get the scheduler expected to be visible to tasks started by the factory.
        /// </summary>
        /// If the factory wasn't created with a scheduler, or it was but has the `HideScheduler` flag set,
        /// this method returns the default (threadpool) scheduler.
        /// Otherwise, it returns the scheduler the factory was created with.
        /// <param name="factory">The factory to get the expected scheduler for.</param>
        /// <returns>The scheduler visible to started tasks.</returns>
        internal static TaskScheduler ExpectedScheduler<T>(this TaskFactory<T> factory)
        {
            return (factory.Scheduler == null || factory.CreationOptions.HasFlag(TaskCreationOptions.HideScheduler) ? TaskScheduler.Default : factory.Scheduler);
        }
    }
}
