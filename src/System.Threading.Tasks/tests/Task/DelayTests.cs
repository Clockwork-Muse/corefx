// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class DelayTests
    {
        [Fact]
        public static void RunDelayTests()
        {
            //
            // Test basic functionality
            //
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            // These should all complete quickly, with RAN_TO_COMPLETION status.
            Task task1 = Task.Delay(0);
            Task task2 = Task.Delay(new TimeSpan(0));
            Task task3 = Task.Delay(0, token);
            Task task4 = Task.Delay(new TimeSpan(0), token);

            Debug.WriteLine("RunDelayTests:    > Waiting for 0-delayed uncanceled tasks to complete.  If we hang, something went wrong.");
            try
            {
                Task.WaitAll(task1, task2, task3, task4);
            }
            catch (Exception e)
            {
                Assert.True(false, string.Format("RunDelayTests:    > FAILED.  Unexpected exception on WaitAll(simple tasks): {0}", e));
            }

            Assert.True(task1.Status == TaskStatus.RanToCompletion, "    > FAILED.  Expected Delay(0) to run to completion");
            Assert.True(task2.Status == TaskStatus.RanToCompletion, "    > FAILED.  Expected Delay(TimeSpan(0)) to run to completion");
            Assert.True(task3.Status == TaskStatus.RanToCompletion, "    > FAILED.  Expected Delay(0,uncanceledToken) to run to completion");
            Assert.True(task4.Status == TaskStatus.RanToCompletion, "    > FAILED.  Expected Delay(TimeSpan(0),uncanceledToken) to run to completion");

            // This should take some time
            Task task7 = Task.Delay(10000);
            Assert.False(task7.IsCompleted, "RunDelayTests:    > FAILED.  Delay(10000) appears to have completed too soon(1).");
            Task t2 = Task.Delay(10);
            Assert.False(task7.IsCompleted, "RunDelayTests:    > FAILED.  Delay(10000) appears to have completed too soon(2).");
        }

        [Fact]
        public static void RunDelayTests_NegativeCases()
        {
            CancellationTokenSource disposedCTS = new CancellationTokenSource();
            CancellationToken disposedToken = disposedCTS.Token;
            disposedCTS.Dispose();

            //
            // Test for exceptions
            //
            Assert.Throws<ArgumentOutOfRangeException>(
               () => { Task.Delay(-2); });
            Assert.Throws<ArgumentOutOfRangeException>(
               () => { Task.Delay(new TimeSpan(1000, 0, 0, 0)); });

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            cts.Cancel();

            // These should complete quickly, in Canceled status
            Task task5 = Task.Delay(0, token);
            Task task6 = Task.Delay(new TimeSpan(0), token);

            Debug.WriteLine("RunDelayTests:    > Waiting for 0-delayed canceled tasks to complete.  If we hang, something went wrong.");
            try
            {
                Task.WaitAll(task5, task6);
            }
            catch { }

            Assert.True(task5.Status == TaskStatus.Canceled, "RunDelayTests:    > FAILED.  Expected Delay(0,canceledToken) to end up Canceled");
            Assert.True(task6.Status == TaskStatus.Canceled, "RunDelayTests:    > FAILED.  Expected Delay(TimeSpan(0),canceledToken) to end up Canceled");

            // Cancellation token on two tasks and waiting on a task a second time.
            CancellationTokenSource cts2 = new CancellationTokenSource();

            Task task8 = Task.Delay(-1, cts2.Token);
            Task task9 = Task.Delay(new TimeSpan(1, 0, 0, 0), cts2.Token);
            Task.Factory.StartNew(() =>
            {
                cts2.Cancel();
            });

            Debug.WriteLine("RunDelayTests:    > Waiting for infinite-delayed, eventually-canceled tasks to complete.  If we hang, something went wrong.");
            try
            {
                Task.WaitAll(task8, task9);
            }
            catch { }

            Assert.True(task8.IsCanceled, "RunDelayTests:    > FAILED.  Expected Delay(-1, token) to end up Canceled.");
            Assert.True(task9.IsCanceled, "RunDelayTests:    > FAILED.  Expected Delay(TimeSpan(1,0,0,0), token) to end up Canceled.");

            try
            {
                task8.Wait();
            }
            catch (AggregateException ae)
            {
                Assert.True(
                   ae.InnerException is OperationCanceledException && ((OperationCanceledException)ae.InnerException).CancellationToken == cts2.Token,
                   "RunDelayTests:    > FAILED.  Expected resulting OCE to contain canceled token.");
            }
        }
    }
}
