// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class RunTests
    {
        [Fact]
        [OuterLoop]
        public static void RunRunTests()
        {
            //
            // Test that AttachedToParent is ignored in Task.Run delegate
            //
            {
                Task tInner = null;

                // Test Run(Action)
                Task t1 = Task.Run(() =>
                {
                    tInner = new Task(() => { }, TaskCreationOptions.AttachedToParent);
                });
                Debug.WriteLine("RunRunTests - AttachToParentIgnored:      -- Waiting on outer Task.  If we hang, that's a failure");
                t1.Wait();
                tInner.Start();
                tInner.Wait();

                // Test Run(Func<int>)
                Task<int> f1 = Task.Run(() =>
                {
                    tInner = new Task(() => { }, TaskCreationOptions.AttachedToParent);
                    return 42;
                });
                Debug.WriteLine("RunRunTests - AttachToParentIgnored:      -- Waiting on outer Task<int>.  If we hang, that's a failure");
                f1.Wait();
                tInner.Start();
                tInner.Wait();

                // Test Run(Func<Task>)
                Task t2 = Task.Run(() =>
                {
                    tInner = new Task(() => { }, TaskCreationOptions.AttachedToParent);
                    Task returnTask = Task.Factory.StartNew(() => { });
                    return returnTask;
                });
                Debug.WriteLine("RunRunTests - AttachToParentIgnored:      -- Waiting on outer Task (unwrap-style).  If we hang, that's a failure");
                t2.Wait();
                tInner.Start();
                tInner.Wait();

                Task<int> fInner = null;
                // Test Run(Func<Task<int>>)
                Task<int> f2 = Task.Run(() =>
                {
                    // Make sure AttachedToParent is ignored for futures as well as tasks
                    fInner = new Task<int>(() => { return 42; }, TaskCreationOptions.AttachedToParent);
                    Task<int> returnTask = Task<int>.Factory.StartNew(() => 11);
                    return returnTask;
                });
                Debug.WriteLine("RunRunTests - AttachToParentIgnored: Waiting on outer Task<int> (unwrap-style).  If we hang, that's a failure");
                f2.Wait();
                fInner.Start();
                fInner.Wait();
            }

            //
            // Test basic functionality w/o cancellation token
            //
            int count = 0;
            Task task1 = Task.Run(() => { count = 1; });
            Debug.WriteLine("RunRunTests: waiting for a task.  If we hang, something went wrong.");
            task1.Wait();
            Assert.True(count == 1, "    > FAILED.  Task completed but did not run.");
            Assert.True(task1.Status == TaskStatus.RanToCompletion, "    > FAILED.  Task did not end in RanToCompletion state.");

            Task<int> future1 = Task.Run(() => { return 7; });
            Debug.WriteLine("RunRunTests - Basic w/o CT: waiting for a future.  If we hang, something went wrong.");
            future1.Wait();
            Assert.True(future1.Result == 7, "    > FAILED.  Future completed but did not run.");
            Assert.True(future1.Status == TaskStatus.RanToCompletion, "    > FAILED.  Future did not end in RanToCompletion state.");

            task1 = Task.Run(() => { return Task.Run(() => { count = 11; }); });
            Debug.WriteLine("RunRunTests - Basic w/o CT: waiting for a task(unwrapped).  If we hang, something went wrong.");
            task1.Wait();
            Assert.True(count == 11, "    > FAILED.  Task(unwrapped) completed but did not run.");
            Assert.True(task1.Status == TaskStatus.RanToCompletion, "    > FAILED.  Task(unwrapped) did not end in RanToCompletion state.");

            future1 = Task.Run(() => { return Task.Run(() => 17); });
            Debug.WriteLine("RunRunTests - Basic w/o CT: waiting for a future(unwrapped).  If we hang, something went wrong.");
            future1.Wait();
            Assert.True(future1.Result == 17, "    > FAILED.  Future(unwrapped) completed but did not run.");
            Assert.True(future1.Status == TaskStatus.RanToCompletion, "    > FAILED.  Future(unwrapped) did not end in RanToCompletion state.");

            //
            // Test basic functionality w/ uncancelled cancellation token
            //
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Task task2 = Task.Run(() => { count = 21; }, token);
            Debug.WriteLine("RunRunTests: waiting for a task w/ uncanceled token.  If we hang, something went wrong.");
            task2.Wait();
            Assert.True(count == 21, "    > FAILED.  Task w/ uncanceled token completed but did not run.");
            Assert.True(task2.Status == TaskStatus.RanToCompletion, "    > FAILED.  Task w/ uncanceled token did not end in RanToCompletion state.");

            Task<int> future2 = Task.Run(() => 27, token);
            Debug.WriteLine("RunRunTests: waiting for a future w/ uncanceled token.  If we hang, something went wrong.");
            future2.Wait();
            Assert.True(future2.Result == 27, "    > FAILED.  Future w/ uncanceled token completed but did not run.");
            Assert.True(future2.Status == TaskStatus.RanToCompletion, "    > FAILED.  Future w/ uncanceled token did not end in RanToCompletion state.");

            task2 = Task.Run(() => { return Task.Run(() => { count = 31; }); }, token);
            Debug.WriteLine("RunRunTests: waiting for a task(unwrapped) w/ uncanceled token.  If we hang, something went wrong.");
            task2.Wait();
            Assert.True(count == 31, "    > FAILED.  Task(unwrapped) w/ uncanceled token completed but did not run.");
            Assert.True(task2.Status == TaskStatus.RanToCompletion, "    > FAILED.  Task(unwrapped) w/ uncanceled token did not end in RanToCompletion state.");

            future2 = Task.Run(() => Task.Run(() => 37), token);
            Debug.WriteLine("RunRunTests: waiting for a future(unwrapped) w/ uncanceled token.  If we hang, something went wrong.");
            future2.Wait();
            Assert.True(future2.Result == 37, "    > FAILED.  Future(unwrapped) w/ uncanceled token completed but did not run.");
            Assert.True(future2.Status == TaskStatus.RanToCompletion, "    > FAILED.  Future(unwrapped) w/ uncanceled token did not end in RanToCompletion state.");
        }

        [Fact]
        [OuterLoop]
        public static void RunRunTests_Cancellation_Negative()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            int count = 0;
            //
            // Test that the right thing is done with a canceled cancellation token
            //
            cts.Cancel();
            Task task3 = Task.Run(() => { count = 41; }, token);
            Debug.WriteLine("RunRunTests: waiting for a task w/ canceled token.  If we hang, something went wrong.");
            Assert.Throws<AggregateException>(
               () => { task3.Wait(); });
            Assert.False(count == 41, "    > FAILED.  Task w/ canceled token ran when it should not have.");
            Assert.True(task3.IsCanceled, "    > FAILED.  Task w/ canceled token should have ended in Canceled state");

            Task future3 = Task.Run(() => { count = 47; return count; }, token);
            Debug.WriteLine("RunRunTests: waiting for a future w/ canceled token.  If we hang, something went wrong.");
            Assert.Throws<AggregateException>(
               () => { future3.Wait(); });
            Assert.False(count == 47, "    > FAILED.  Future w/ canceled token ran when it should not have.");
            Assert.True(future3.IsCanceled, "    > FAILED.  Future w/ canceled token should have ended in Canceled state");

            task3 = Task.Run(() => { return Task.Run(() => { count = 51; }); }, token);
            Debug.WriteLine("RunRunTests: waiting for a task(unwrapped) w/ canceled token.  If we hang, something went wrong.");
            Assert.Throws<AggregateException>(
               () => { task3.Wait(); });
            Assert.False(count == 51, "    > FAILED.  Task(unwrapped) w/ canceled token ran when it should not have.");
            Assert.True(task3.IsCanceled, "    > FAILED.  Task(unwrapped) w/ canceled token should have ended in Canceled state");

            future3 = Task.Run(() => { return Task.Run(() => { count = 57; return count; }); }, token);
            Debug.WriteLine("RunRunTests: waiting for a future(unwrapped) w/ canceled token.  If we hang, something went wrong.");
            Assert.Throws<AggregateException>(
               () => { future3.Wait(); });
            Assert.False(count == 57, "    > FAILED.  Future(unwrapped) w/ canceled token ran when it should not have.");
            Assert.True(future3.IsCanceled, "    > FAILED.  Future(unwrapped) w/ canceled token should have ended in Canceled state");
        }

        [Fact]
        public static void RunRunTests_FastPathTests()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            CancellationToken token = cts.Token;

            //
            // Test that "fast paths" operate correctly
            //
            {
                // Create some pre-completed Tasks
                Task alreadyCompletedTask = Task.Factory.StartNew(() => { });
                alreadyCompletedTask.Wait();

                Task alreadyFaultedTask = Task.Factory.StartNew(() => { throw new Exception("FAULTED!"); });
                try { alreadyFaultedTask.Wait(); }
                catch { }

                Task alreadyCanceledTask = new Task(() => { }, cts.Token); // should result in cancellation
                try { alreadyCanceledTask.Wait(); }
                catch { }

                // Now run them through Task.Run
                Task fastPath1 = Task.Run(() => alreadyCompletedTask);
                fastPath1.Wait();
                Assert.True(fastPath1.Status == TaskStatus.RanToCompletion,
                   "RunRunTests: Expected proxy for already-ran-to-completion task to be in RanToCompletion status");

                fastPath1 = Task.Run(() => alreadyFaultedTask);
                try
                {
                    fastPath1.Wait();
                    Assert.True(false, string.Format("RunRunTests:    > FAILURE: Expected proxy for already-faulted Task to throw on Wait()"));
                }
                catch { }
                Assert.True(fastPath1.Status == TaskStatus.Faulted, "Expected proxy for already-faulted task to be in Faulted status");

                fastPath1 = Task.Run(() => alreadyCanceledTask);
                try
                {
                    fastPath1.Wait();
                    Assert.True(false, string.Format("RunRunTests:    > FAILURE: Expected proxy for already-canceled Task to throw on Wait()"));
                }
                catch { }
                Assert.True(fastPath1.Status == TaskStatus.Canceled, "RunRunTests: Expected proxy for already-canceled task to be in Canceled status");
            }
            {
                // Create some pre-completed Task<int>s
                Task<int> alreadyCompletedTask = Task<int>.Factory.StartNew(() => 42);
                alreadyCompletedTask.Wait();
                bool doIt = true;

                Task<int> alreadyFaultedTask = Task<int>.Factory.StartNew(() => { if (doIt) throw new Exception("FAULTED!"); return 42; });
                try { alreadyFaultedTask.Wait(); }
                catch { }

                Task<int> alreadyCanceledTask = new Task<int>(() => 42, cts.Token); // should result in cancellation
                try { alreadyCanceledTask.Wait(); }
                catch { }

                // Now run them through Task.Run
                Task<int> fastPath1 = Task.Run(() => alreadyCompletedTask);
                fastPath1.Wait();
                Assert.True(fastPath1.Status == TaskStatus.RanToCompletion, "RunRunTests: Expected proxy for already-ran-to-completion future to be in RanToCompletion status");

                fastPath1 = Task.Run(() => alreadyFaultedTask);
                try
                {
                    fastPath1.Wait();
                    Assert.True(false, string.Format("RunRunTests:    > FAILURE: Expected proxy for already-faulted future to throw on Wait()"));
                }
                catch { }
                Assert.True(fastPath1.Status == TaskStatus.Faulted, "Expected proxy for already-faulted future to be in Faulted status");

                fastPath1 = Task.Run(() => alreadyCanceledTask);
                try
                {
                    fastPath1.Wait();
                    Assert.True(false, string.Format("RunRunTests:    > FAILURE: Expected proxy for already-canceled future to throw on Wait()"));
                }
                catch { }
                Assert.True(fastPath1.Status == TaskStatus.Canceled, "RunRunTests: Expected proxy for already-canceled future to be in Canceled status");
            }
        }

        [Fact]
        public static void RunRunTests_Unwrap_NegativeCases()
        {
            //
            // Test cancellation/exceptions behavior in the unwrap overloads
            //
            Action<UnwrappedScenario> TestUnwrapped =
                delegate (UnwrappedScenario scenario)
                {
                    Debug.WriteLine("RunRunTests: testing Task unwrap (scenario={0})", scenario);

                    CancellationTokenSource cts1 = new CancellationTokenSource();
                    CancellationToken token1 = cts1.Token;

                    int something = 0;
                    Task t1 = Task.Run(() =>
                    {
                        if (scenario == UnwrappedScenario.ThrowExceptionInDelegate) throw new Exception("thrownInDelegate");
                        if (scenario == UnwrappedScenario.ThrowOceInDelegate) throw new OperationCanceledException("thrownInDelegate");
                        return Task.Run(() =>
                        {
                            if (scenario == UnwrappedScenario.ThrowExceptionInTask) throw new Exception("thrownInTask");
                            if (scenario == UnwrappedScenario.ThrowTargetOceInTask) { cts1.Cancel(); throw new OperationCanceledException(token1); }
                            if (scenario == UnwrappedScenario.ThrowOtherOceInTask) throw new OperationCanceledException(CancellationToken.None);
                            something = 1;
                        }, token1);
                    });

                    bool cancellationExpected = (scenario == UnwrappedScenario.ThrowOceInDelegate) ||
                                                (scenario == UnwrappedScenario.ThrowTargetOceInTask);
                    bool exceptionExpected = (scenario == UnwrappedScenario.ThrowExceptionInDelegate) ||
                                             (scenario == UnwrappedScenario.ThrowExceptionInTask) ||
                                             (scenario == UnwrappedScenario.ThrowOtherOceInTask);
                    try
                    {
                        t1.Wait();
                        Assert.False(cancellationExpected || exceptionExpected, "TaskRtTests.RunRunTests: Expected exception or cancellation");
                        Assert.True(something == 1, "TaskRtTests.RunRunTests: Task completed but apparantly did not run");
                    }
                    catch (AggregateException ae)
                    {
                        Assert.True(cancellationExpected || exceptionExpected, "TaskRtTests.RunRunTests: Didn't expect exception, got " + ae);
                    }

                    if (cancellationExpected)
                    {
                        Assert.True(t1.IsCanceled, "TaskRtTests.RunRunTests: Expected t1 to be Canceled, was " + t1.Status);
                    }
                    else if (exceptionExpected)
                    {
                        Assert.True(t1.IsFaulted, "TaskRtTests.RunRunTests: Expected t1 to be Faulted, was " + t1.Status);
                    }
                    else
                    {
                        Assert.True(t1.Status == TaskStatus.RanToCompletion, "TaskRtTests.RunRunTests: Expected t1 to be RanToCompletion, was " + t1.Status);
                    }

                    Debug.WriteLine("RunRunTests: -- testing Task<int> unwrap (scenario={0})", scenario);

                    CancellationTokenSource cts2 = new CancellationTokenSource();
                    CancellationToken token2 = cts2.Token;

                    Task<int> f1 = Task.Run(() =>
                    {
                        if (scenario == UnwrappedScenario.ThrowExceptionInDelegate) throw new Exception("thrownInDelegate");
                        if (scenario == UnwrappedScenario.ThrowOceInDelegate) throw new OperationCanceledException("thrownInDelegate");
                        return Task.Run(() =>
                        {
                            if (scenario == UnwrappedScenario.ThrowExceptionInTask) throw new Exception("thrownInTask");
                            if (scenario == UnwrappedScenario.ThrowTargetOceInTask) { cts2.Cancel(); throw new OperationCanceledException(token2); }
                            if (scenario == UnwrappedScenario.ThrowOtherOceInTask) throw new OperationCanceledException(CancellationToken.None);
                            return 10;
                        }, token2);
                    });

                    try
                    {
                        f1.Wait();
                        Assert.False(cancellationExpected || exceptionExpected, "RunRunTests: Expected exception or cancellation");
                        Assert.True(f1.Result == 10, "RunRunTests: Expected f1.Result to be 10, and it was " + f1.Result);
                    }
                    catch (AggregateException ae)
                    {
                        Assert.True(cancellationExpected || exceptionExpected, "RunRunTests: Didn't expect exception, got " + ae);
                    }

                    if (cancellationExpected)
                    {
                        Assert.True(f1.IsCanceled, "RunRunTests: Expected f1 to be Canceled, was " + f1.Status);
                    }
                    else if (exceptionExpected)
                    {
                        Assert.True(f1.IsFaulted, "RunRunTests: Expected f1 to be Faulted, was " + f1.Status);
                    }
                    else
                    {
                        Assert.True(f1.Status == TaskStatus.RanToCompletion, "RunRunTests: Expected f1 to be RanToCompletion, was " + f1.Status);
                    }
                };

            TestUnwrapped(UnwrappedScenario.CleanRun); // no exceptions or cancellation
            TestUnwrapped(UnwrappedScenario.ThrowExceptionInDelegate); // exception in delegate
            TestUnwrapped(UnwrappedScenario.ThrowOceInDelegate); // delegate throws OCE
            TestUnwrapped(UnwrappedScenario.ThrowExceptionInTask); // user-produced Task throws exception
            TestUnwrapped(UnwrappedScenario.ThrowTargetOceInTask); // user-produced Task throws OCE(target)
            TestUnwrapped(UnwrappedScenario.ThrowOtherOceInTask); // user-produced Task throws OCE(random)
        }

        internal enum UnwrappedScenario
        {
            CleanRun = 0,
            ThrowExceptionInDelegate = 1,
            ThrowOceInDelegate = 2,
            ThrowExceptionInTask = 3,
            ThrowTargetOceInTask = 4,
            ThrowOtherOceInTask = 5
        };
    }
}
