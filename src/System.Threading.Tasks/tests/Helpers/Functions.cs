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
    }
}
