// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class IAsyncWaitHandleTests
    {
        [Fact]
        public static void RunAsyncWaitHandleTests()
        {
            // Start a task, but make sure that it does not complete
            ManualResetEvent mre = new ManualResetEvent(false);
            Task t1 = Task.Factory.StartNew(() => { mre.WaitOne(); });

            // Make sure that waiting on an uncompleted Task's AsyncWaitHandle does not succeed
            WaitHandle wh = ((IAsyncResult)t1).AsyncWaitHandle;
            Assert.False(wh.WaitOne(0), "RunAsyncWaitHandleTests:  Did not expect wait on uncompleted Task's AsyncWaitHandle to succeed");

            // Make sure that waiting on a completed Task's AsyncWaitHandle succeeds
            mre.Set();
            Assert.True(wh.WaitOne(), "RunAsyncWaitHandleTests:  Expected wait on completed Task's AsyncWaitHandle to succeed");

            // To complete coverage for CompletedEvent_get, we need to grab a fresh AsyncWaitHandle from
            // an already-completed Task.
            t1 = Task.Factory.StartNew(() => { });
            t1.Wait();
            wh = ((IAsyncResult)t1).AsyncWaitHandle;
            Assert.True(wh.WaitOne(0), "RunAsyncWaitHandleTests:  Expected wait on AsyncWaitHandle from completed Task to succeed");
        }
    }
}
