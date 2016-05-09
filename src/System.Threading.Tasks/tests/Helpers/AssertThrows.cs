// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace System.Threading.Tasks.Tests
{
    internal static class AssertThrows
    {
        /// <summary>
        /// Verify that a single exception of the given type is thrown and wrapped by AggregateException.
        /// </summary>
        /// <typeparam name="T">The type of the inner exceptions</typeparam>
        /// <param name="query">The action throwing an exception.</param>
        public static void Wrapped<T>(Action query) where T : Exception
        {
            AggregateException ae = Assert.Throws<AggregateException>(query);
            Exception single = Assert.Single(ae.InnerExceptions);
            Assert.IsType<T>(single);
        }

        /// <summary>
        /// Verify the count of inner exceptions thrown,
        /// and that all exceptions wrapped by AggregateException match the given type.
        /// </summary>
        /// <typeparam name="T">The type of the inner exceptions</typeparam>
        /// <param name="query">The action throwing an exception.</param>
        /// <param name="innerCount">The count of inner exceptions.</param>
        public static void Wrapped<T>(Action query, int innerCount) where T : Exception
        {
            AggregateException ae = Assert.Throws<AggregateException>(query);
            Assert.Equal(innerCount, ae.InnerExceptions.Count);
            Assert.All(ae.InnerExceptions, e => Assert.IsType<T>(e));
        }

        /// <summary>
        /// Verify that the constructed task throws the given exception during evaluation,
        /// and returns the faulted task for further examination.
        /// </summary>
        /// <remarks>The caller MUST use await or the check will not occur.</remarks>
        /// <typeparam name="T">The expected (single) exception.</typeparam>
        /// <param name="query">A func that returns a (running) task.</param>
        /// <returns>The faulted task, for further evaluation.</returns>
        public async static Task<Task> Async<T>(Func<Task> query) where T : Exception
        {
            Task t = query();
            await Assert.ThrowsAsync<T>(() => t);
            return t;
        }
    }
}
