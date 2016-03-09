// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Threading.Tasks.Tests
{
    internal static class Spin
    {
        private static readonly TimeSpan MaxSafeWait = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Spin until the given condition is true, throws an exception if the time limit is reached.
        /// </summary>
        /// Time limit is one second
        /// <param name="condition">The condition to check.</param>
        public static void UntilOrTimeout(Func<bool> condition)
        {
            UntilOrTimeout(condition, MaxSafeWait);
        }

        /// <summary>
        /// Spin until the given condition is true, throws an exception if the time limit is reached.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="timeout">The maximum amount of time to wait before throwing.</param>
        public static void UntilOrTimeout(Func<bool> condition, TimeSpan timeout)
        {
            bool succeeded = SpinWait.SpinUntil(condition, timeout);
            if (!succeeded) throw new TimeoutException();
        }
    }
}
