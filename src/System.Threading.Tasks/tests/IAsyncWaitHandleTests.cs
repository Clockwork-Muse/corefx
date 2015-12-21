// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class IAsyncWaitHandleTests
    {
        [Fact]
        public static void RunningTask_Timeout()
        {
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                // Start a task, but make sure that it does not complete
                Task running = new TaskFactory().StartNew(() => { mre.WaitOne(); });

                WaitHandle handle = ((IAsyncResult)running).AsyncWaitHandle;

                // Make sure that waiting on an uncompleted Task's AsyncWaitHandle does not succeed
                Assert.False(handle.WaitOne(0));
                Assert.False(handle.WaitOne(TimeSpan.Zero));
                Assert.False(handle.WaitOne(1));
                Assert.False(handle.WaitOne(TimeSpan.FromMilliseconds(1)));

                // Make sure that waiting on a completed Task's AsyncWaitHandle (eventually) succeeds
                mre.Set();
                Assert.True(handle.WaitOne());
            }
        }

        [Fact]
        public static void CompletedTask_Wait()
        {
            // Waiting on an already-completed task should succeed immediately.
            Task completed = new TaskFactory().StartNew(() => { });
            completed.Wait();

            WaitHandle handle = ((IAsyncResult)completed).AsyncWaitHandle;

            Assert.True(handle.WaitOne());
            Assert.True(handle.WaitOne(0));
            Assert.True(handle.WaitOne(TimeSpan.Zero));
            Assert.True(handle.WaitOne(1));
            Assert.True(handle.WaitOne(TimeSpan.FromMilliseconds(1)));
        }
    }
}
