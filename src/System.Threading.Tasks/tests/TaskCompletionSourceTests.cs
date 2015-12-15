// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class TaskCompletionSourceTests
    {
        private const int ConcurrencyLevel = 50;

        /// <summary>
        /// Get a member which can be used to run the same test multiple times.
        /// </summary>
        /// <param name="numberOfRuns">The number of times to run a particular test</param>
        /// <returns>An array containing the id of the test run</returns>
        public static IEnumerable<object[]> RunMultipleTimes(int numberOfRuns)
        {
            for (int run = 1; run < numberOfRuns + 1; run++)
            {
                yield return new object[] { run };
            }
        }

        /// <summary>
        /// Get constructed TaskCompletionSource
        /// </summary>
        /// Format is:
        ///  1. Creation func: takes object state and options, which may be ignored
        ///  2. optional object state
        ///  3. optional TaskCreationOptions
        /// <returns>Row of data</returns>
        public static IEnumerable<object[]> Constructors()
        {
            yield return new object[] { (Func<object, TaskCreationOptions?, TaskCompletionSource<int>>)
                ((state, options) => new TaskCompletionSource<int>()), null, null };
            yield return new object[] { (Func<object, TaskCreationOptions?, TaskCompletionSource<int>>)
                ((state, options) => new TaskCompletionSource<int>(state)), new object(), null };
            foreach (TaskCreationOptions option in new[] { TaskCreationOptions.None,
                TaskCreationOptions.AttachedToParent, TaskCreationOptions.RunContinuationsAsynchronously,
                TaskCreationOptions.AttachedToParent | TaskCreationOptions.RunContinuationsAsynchronously })
            {
                yield return new object[] { (Func<object, TaskCreationOptions?, TaskCompletionSource<int>>)
                ((state, options) => new TaskCompletionSource<int>(options.Value)), null, option };
                yield return new object[] { (Func<object, TaskCreationOptions?, TaskCompletionSource<int>>)
                ((state, options) => new TaskCompletionSource<int>(state, options.Value)), new object(), option };
            }
        }

        [Theory]
        [MemberData("Constructors")]
        public static void Constructor_Test(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            TaskCompletionSource<int> completionSource = create(state, options);

            Assert.NotNull(completionSource);

            Task<int> task = completionSource.Task;
            Assert.NotNull(task);
            Assert.False(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
            Assert.Null(task.Exception);
            Assert.Equal(options.GetValueOrDefault(), task.CreationOptions);
            Assert.Equal(state, task.AsyncState);
            Assert.Equal(TaskStatus.WaitingForActivation, task.Status);
        }

        [Theory]
        [MemberData("Constructors")]
        public static void SetResult_Test(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            int expectedResult = 5;

            TaskCompletionSource<int> completionSource = create(state, options);

            completionSource.SetResult(expectedResult);

            Task<int> task = completionSource.Task;

            AssertFinished(completionSource, () => AssertComplete(expectedResult, task));
        }

        private static void AssertFinished(TaskCompletionSource<int> completionSource, Action taskVerification)
        {
            taskVerification();

            Assert.False(completionSource.TrySetResult(-1));
            Assert.False(completionSource.TrySetCanceled());
            Assert.False(completionSource.TrySetCanceled(new CancellationTokenSource().Token));
            Assert.False(completionSource.TrySetException(new DeliberateTestException()));
            Assert.False(completionSource.TrySetException(new[] { new DeliberateTestException() }));

            // run verification multiple times to ensure state of task hasn't changed despite potential exceptions.
            taskVerification();

            Assert.Throws<InvalidOperationException>(() => completionSource.SetResult(-1));
            Assert.Throws<InvalidOperationException>(() => completionSource.SetCanceled());
            Assert.Throws<InvalidOperationException>(() => completionSource.SetException(new DeliberateTestException()));
            Assert.Throws<InvalidOperationException>(() => completionSource.SetException(new[] { new DeliberateTestException() }));

            taskVerification();
        }

        // Make sure that TaskCompletionSource/TaskCompletionSource.Task handle state changes correctly.
        [Theory]
        [MemberData("Constructors")]
        public static void TrySetResult_Test(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            int expectedResult = 5;

            TaskCompletionSource<int> completionSource = create(state, options);

            Assert.True(completionSource.TrySetResult(expectedResult));

            Task<int> task = completionSource.Task;

            AssertFinished(completionSource, () => AssertComplete(expectedResult, task));
        }

        private static void AssertComplete(int expectedResult, Task<int> task)
        {
            Assert.NotNull(task);
            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
            Assert.Null(task.Exception);
            Assert.Equal(expectedResult, task.Result);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        }

        [Theory]
        [OuterLoop]
        [MemberData("Constructors")]
        public static void TrySetResult_ExplicitWait(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            int expectedResult = 42;
            Finished_ImplicitExplicitWaits(create, state, options,
                completionSource => completionSource.TrySetResult(expectedResult),
                task =>
                {
                    task.Wait();
                    AssertComplete(expectedResult, task);
                });
        }

        [Theory]
        [OuterLoop]
        [MemberData("Constructors")]
        public static void TrySetResult_ImplicitWait(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            int expectedResult = 42;
            Finished_ImplicitExplicitWaits(create, state, options,
                completionSource => completionSource.TrySetResult(expectedResult),
                task =>
                {
                    Assert.Equal(expectedResult, task.Result);
                    AssertComplete(expectedResult, task);
                });
        }

        private static void Finished_ImplicitExplicitWaits(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options,
            Action<TaskCompletionSource<int>> finish, Action<Task<int>> waitAndVerification)
        {
            TaskCompletionSource<int> completionSource = create(state, options);
            Task<int> task = completionSource.Task;

            using (Barrier startingLine = new Barrier(2))
            {
                Task.Run(() =>
                {
                    startingLine.SignalAndWait();
                    Task.Delay(TimeSpan.FromMilliseconds(5)).Wait();
                    finish(completionSource);
                });

                // use a barrier to synchronize, ensure both running
                startingLine.SignalAndWait();
            }
            waitAndVerification(task);
        }

        [Theory]
        [OuterLoop]
        [MemberData("RunMultipleTimes", 20)]
        // Test takes significant time due to forcing the thread pool to create additional threads.
        public static void TrySetResult_Concurrent_Test(int testRunNumber)
        {
            using (Barrier startingLine = new Barrier(ConcurrencyLevel))
            {
                int expectedResult = 10;
                TaskCompletionSource<int> completionSource = new TaskCompletionSource<int>();
                int didNotSet = 0;

                Task[] tasks = new Task[ConcurrencyLevel];
                for (int i = 0; i < ConcurrencyLevel; i++)
                {
                    tasks[i] = new Task(() =>
                     {
                         // Force all threads to be running and execute at the same time.
                         startingLine.SignalAndWait();
                         bool succeeded = completionSource.TrySetResult(expectedResult);
                         Assert.Equal(expectedResult, completionSource.Task.Result);

                         if (!succeeded) Interlocked.Increment(ref didNotSet);
                     });
                }

                tasks.StartAll();

                Task.WaitAll(tasks);
                Assert.Equal(ConcurrencyLevel - 1, didNotSet);
            }
        }

        [Theory]
        [MemberData("Constructors")]
        public static void SetCanceled_Test(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            TaskCompletionSource<int> completionSource = create(state, options);

            completionSource.SetCanceled();

            Task<int> task = completionSource.Task;

            AssertFinished(completionSource, () => AssertCanceled(new CancellationToken(false), task));
        }

        [Theory]
        [MemberData("Constructors")]
        public static void TrySetCanceled_Test(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            TaskCompletionSource<int> completionSource = create(state, options);

            Assert.True(completionSource.TrySetCanceled());

            Task<int> task = completionSource.Task;

            AssertFinished(completionSource, () => AssertCanceled(new CancellationToken(false), task));
        }

        private static void AssertCanceled(CancellationToken token, Task<int> task)
        {
            Assert.NotNull(task);
            Assert.True(task.IsCompleted);
            Assert.True(task.IsCanceled);
            Assert.False(task.IsFaulted);
            Assert.Null(task.Exception);
            Assert.Equal(TaskStatus.Canceled, task.Status);
            AggregateException resultException = Assert.Throws<AggregateException>(() => task.Result);
            AggregateException waitException = Assert.Throws<AggregateException>(() => task.Wait());
            TaskCanceledException tce = Assert.IsType<TaskCanceledException>(resultException.InnerException);
            Assert.Equal(token, tce.CancellationToken);
            tce = Assert.IsType<TaskCanceledException>(waitException.InnerException);
            Assert.Equal(token, tce.CancellationToken);
        }

        [Theory]
        [OuterLoop]
        [MemberData("Constructors")]
        public static void TrySetCanceled_ExplicitWait(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            Finished_ImplicitExplicitWaits(create, state, options,
                completionSource => completionSource.TrySetCanceled(),
                task =>
                {
                    Assert.Throws<AggregateException>(() => task.Wait());
                    AssertCanceled(new CancellationToken(false), task);
                });
        }

        [Theory]
        [OuterLoop]
        [MemberData("Constructors")]
        public static void TrySetCanceled_ImplicitWait(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            Finished_ImplicitExplicitWaits(create, state, options,
                completionSource => completionSource.TrySetCanceled(),
                task =>
                {
                    Assert.Throws<AggregateException>(() => task.Result);
                    AssertCanceled(new CancellationToken(false), task);
                });
        }

        [Theory]
        [OuterLoop]
        [MemberData("RunMultipleTimes", 20)]
        // Test takes significant time due to forcing the thread pool to create additional threads.
        public static void TrySetCanceled_Concurrent_Test(int testRunNumber)
        {
            using (Barrier startingLine = new Barrier(ConcurrencyLevel))
            {
                TaskCompletionSource<int> completionSource = new TaskCompletionSource<int>();
                int didNotSet = 0;

                Task[] tasks = new Task[ConcurrencyLevel];
                for (int i = 0; i < ConcurrencyLevel; i++)
                {
                    tasks[i] = new Task(() =>
                    {
                        Assert.False(completionSource.Task.IsCanceled);
                        // Force all threads to be running and execute at the same time.
                        startingLine.SignalAndWait();
                        bool succeeded = completionSource.TrySetCanceled();
                        Assert.True(completionSource.Task.IsCanceled);

                        if (!succeeded) Interlocked.Increment(ref didNotSet);
                    });
                }

                tasks.StartAll();

                Task.WaitAll(tasks);
                Assert.Equal(ConcurrencyLevel - 1, didNotSet);
            }
        }

        [Theory]
        [MemberData("Constructors")]
        public static void TrySetCanceled_Token_Test(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            TaskCompletionSource<int> completionSource = create(state, options);
            CancellationTokenSource source = new CancellationTokenSource();
            source.Cancel();

            Assert.True(completionSource.TrySetCanceled(source.Token));

            Task<int> task = completionSource.Task;

            AssertFinished(completionSource, () => AssertCanceled(source.Token, task));
        }

        [Theory]
        [OuterLoop]
        [MemberData("Constructors")]
        public static void TrySetCanceled_Token_ExplicitWait(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Finished_ImplicitExplicitWaits(create, state, options,
                completionSource => completionSource.TrySetCanceled(source.Token),
                task =>
                {
                    Assert.Throws<AggregateException>(() => task.Wait());
                    AssertCanceled(source.Token, task);
                });
        }

        [Theory]
        [OuterLoop]
        [MemberData("Constructors")]
        public static void TrySetCanceled_Token_ImplicitWait(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Finished_ImplicitExplicitWaits(create, state, options,
                completionSource => completionSource.TrySetCanceled(source.Token),
                task =>
                {
                    Assert.Throws<AggregateException>(() => task.Result);
                    AssertCanceled(source.Token, task);
                });
        }

        [Theory]
        [OuterLoop]
        [MemberData("RunMultipleTimes", 20)]
        // Test takes significant time due to forcing the thread pool to create additional threads.
        public static void TrySetCanceled_Token_Concurrent_Test(int testRunNumber)
        {
            using (Barrier startingLine = new Barrier(ConcurrencyLevel))
            {
                CancellationTokenSource source = new CancellationTokenSource();
                source.Cancel();

                TaskCompletionSource<int> completionSource = new TaskCompletionSource<int>();
                int didNotSet = 0;

                Task[] tasks = new Task[ConcurrencyLevel];
                for (int i = 0; i < ConcurrencyLevel; i++)
                {
                    tasks[i] = new Task(() =>
                    {
                        Assert.False(completionSource.Task.IsCanceled);
                        // Force all threads to be running and execute at the same time.
                        startingLine.SignalAndWait();
                        bool succeeded = completionSource.TrySetCanceled(source.Token);
                        Assert.True(completionSource.Task.IsCanceled);

                        if (!succeeded) Interlocked.Increment(ref didNotSet);
                    });
                }

                tasks.StartAll();

                Task.WaitAll(tasks);
                Assert.Equal(ConcurrencyLevel - 1, didNotSet);
            }
        }

        [Theory]
        [OuterLoop]
        [MemberData("Constructors")]
        public static void SetException_ImplicitWait(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            DeliberateTestException dte = new DeliberateTestException();
            Finished_ImplicitExplicitWaits(create, state, options,
                completionSource => completionSource.SetException(dte),
                task =>
                {
                    Assert.Throws<AggregateException>(() => task.Result);
                    AssertFaulted(dte, task);
                });
        }

        [Theory]
        [MemberData("Constructors")]
        public static void TrySetException_Test(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            TaskCompletionSource<int> completionSource = create(state, options);
            DeliberateTestException testException = new DeliberateTestException();

            Assert.True(completionSource.TrySetException(testException));

            Task<int> task = completionSource.Task;

            AssertFinished(completionSource, () => AssertFaulted(testException, task));
        }

        [Theory]
        [OuterLoop]
        [MemberData("Constructors")]
        public static void TrySetException_ExplicitWait(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            DeliberateTestException dte = new DeliberateTestException();
            Finished_ImplicitExplicitWaits(create, state, options,
                completionSource => completionSource.TrySetException(dte),
                task =>
                {
                    Assert.Throws<AggregateException>(() => task.Wait());
                    AssertFaulted(dte, task);
                });
        }

        [Theory]
        [OuterLoop]
        [MemberData("Constructors")]
        public static void TrySetException_ImplicitWait(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            DeliberateTestException dte = new DeliberateTestException();
            Finished_ImplicitExplicitWaits(create, state, options,
                completionSource => completionSource.TrySetException(dte),
                task =>
                {
                    Assert.Throws<AggregateException>(() => task.Result);
                    AssertFaulted(dte, task);
                });
        }

        private static void AssertFaulted<T>(T exception, Task<int> task) where T : Exception
        {
            Assert.NotNull(task);
            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.True(task.IsFaulted);
            Assert.NotNull(task.Exception);
            Assert.Equal(TaskStatus.Faulted, task.Status);
            AggregateException resultException = Assert.Throws<AggregateException>(() => task.Result);
            AggregateException waitException = Assert.Throws<AggregateException>(() => task.Wait());
            Assert.Equal(exception, task.Exception.InnerException);
            Assert.Equal(exception, resultException.InnerException);
            Assert.Equal(exception, waitException.InnerException);
        }

        [Theory]
        [OuterLoop]
        [MemberData("RunMultipleTimes", 20)]
        // Test takes significant time due to forcing the thread pool to create additional threads.
        public static void TrySetException_Concurrent_Test(int testRunNumber)
        {
            using (Barrier startingLine = new Barrier(ConcurrencyLevel))
            {
                TaskCompletionSource<int> completionSource = new TaskCompletionSource<int>();
                int didNotSet = 0;

                Task[] tasks = new Task[ConcurrencyLevel];
                for (int i = 0; i < ConcurrencyLevel; i++)
                {
                    tasks[i] = new Task(() =>
                    {
                        Assert.False(completionSource.Task.IsFaulted);
                        Assert.Null(completionSource.Task.Exception);
                        // Force all threads to be running and execute at the same time.
                        startingLine.SignalAndWait();
                        bool succeeded = completionSource.TrySetException(new DeliberateTestException());
                        Assert.True(completionSource.Task.IsFaulted);
                        Assert.NotNull(completionSource.Task.Exception);

                        if (!succeeded) Interlocked.Increment(ref didNotSet);
                    });
                }

                tasks.StartAll();

                Task.WaitAll(tasks);
                Assert.Equal(ConcurrencyLevel - 1, didNotSet);
            }
        }

        [Theory]
        [MemberData("Constructors")]
        public static void SetException_Multiple_Test(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            TaskCompletionSource<int> completionSource = create(state, options);
            Exception[] exceptions = new[] { new DeliberateTestException(), new DeliberateTestException() };

            completionSource.SetException(exceptions);

            Task<int> task = completionSource.Task;

            AssertFinished(completionSource, () => AssertFaulted(exceptions, task));
        }

        [Theory]
        [MemberData("Constructors")]
        public static void TrySetException_Multiple_Test(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            TaskCompletionSource<int> completionSource = create(state, options);
            Exception[] exceptions = new[] { new DeliberateTestException(), new DeliberateTestException() };

            Assert.True(completionSource.TrySetException(exceptions));

            Task<int> task = completionSource.Task;

            AssertFinished(completionSource, () => AssertFaulted(exceptions, task));
        }

        private static void AssertFaulted(Exception[] exceptions, Task<int> task)
        {
            Assert.NotNull(task);
            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.True(task.IsFaulted);
            Assert.NotNull(task.Exception);
            Assert.Equal(TaskStatus.Faulted, task.Status);
            AggregateException resultException = Assert.Throws<AggregateException>(() => task.Result);
            AggregateException waitException = Assert.Throws<AggregateException>(() => task.Wait());
            Assert.Equal(exceptions, task.Exception.InnerExceptions);
            Assert.Equal(exceptions, resultException.InnerExceptions);
            Assert.Equal(exceptions, waitException.InnerExceptions);
        }

        [Theory]
        [OuterLoop]
        [MemberData("Constructors")]
        public static void TrySetException_Multiple_ExplicitWait(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            Exception[] exceptions = new[] { new DeliberateTestException(), new DeliberateTestException() };
            Finished_ImplicitExplicitWaits(create, state, options,
                completionSource => completionSource.TrySetException(exceptions),
                task =>
                {
                    Assert.Throws<AggregateException>(() => task.Wait());
                    AssertFaulted(exceptions, task);
                });
        }

        [Theory]
        [OuterLoop]
        [MemberData("Constructors")]
        public static void TrySetException_Multiple_ImplicitWait(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            Exception[] exceptions = new[] { new DeliberateTestException(), new DeliberateTestException() };
            Finished_ImplicitExplicitWaits(create, state, options,
                completionSource => completionSource.TrySetException(exceptions),
                task =>
                {
                    Assert.Throws<AggregateException>(() => task.Result);
                    AssertFaulted(exceptions, task);
                });
        }

        [Theory]
        [OuterLoop]
        [MemberData("RunMultipleTimes", 20)]
        // Test takes significant time due to forcing the thread pool to create additional threads.
        public static void TrySetException_Multiple_Concurrent_Test(int testRunNumber)
        {
            using (Barrier startingLine = new Barrier(ConcurrencyLevel))
            {
                IEnumerable<Exception> exceptions = Enumerable.Repeat(new DeliberateTestException(), 10);

                TaskCompletionSource<int> completionSource = new TaskCompletionSource<int>();
                int didNotSet = 0;

                Task[] tasks = new Task[ConcurrencyLevel];
                for (int i = 0; i < ConcurrencyLevel; i++)
                {
                    tasks[i] = new Task(() =>
                    {
                        Assert.False(completionSource.Task.IsFaulted);
                        Assert.Null(completionSource.Task.Exception);
                        // Force all threads to be running and execute at the same time.
                        startingLine.SignalAndWait();
                        bool succeeded = completionSource.TrySetException(exceptions);
                        Assert.True(completionSource.Task.IsFaulted);
                        Assert.NotNull(completionSource.Task.Exception);

                        if (!succeeded) Interlocked.Increment(ref didNotSet);
                    });
                }

                tasks.StartAll();

                Task.WaitAll(tasks);
                Assert.Equal(ConcurrencyLevel - 1, didNotSet);
            }
        }

        [Theory]
        [MemberData("Constructors")]
        // Simple continuation test.  General Task.ContinueWith tests are covered elsewhere.
        public static void Task_Continuation(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            int expectedResult = 42;
            TaskCompletionSource<int> completionSource = create(state, options);
            completionSource.SetResult(expectedResult);
            Task<bool> continuation = completionSource.Task.ContinueWith(task =>
            {
                AssertComplete(expectedResult, task);
                return true;
            });
            Assert.True(continuation.Result);
        }

        [Theory]
        [MemberData("Constructors")]
        public static void TrySetCanceled_NotCanceledToken(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            TaskCompletionSource<int> completionSource = create(state, options);
            completionSource.TrySetCanceled(new CancellationToken(false));
            Assert.True(completionSource.Task.IsCanceled);

            TaskCanceledException tce = Assert.Throws<AggregateException>(() => completionSource.Task.Wait()).InnerException as TaskCanceledException;
            Assert.False(tce.CancellationToken.IsCancellationRequested);
        }

        [Theory]
        [MemberData("Constructors")]
        public static void TrySetCanceled_CanceledToken(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            TaskCompletionSource<int> completionSource = create(state, options);
            completionSource.TrySetCanceled(new CancellationToken(true));
            Assert.True(completionSource.Task.IsCanceled);

            TaskCanceledException tce = Assert.Throws<AggregateException>(() => completionSource.Task.Wait()).InnerException as TaskCanceledException;
            Assert.True(tce.CancellationToken.IsCancellationRequested);
        }

        [Theory]
        [MemberData("Constructors")]
        public static void TrySetCanceled_SourceNotCanceled(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            Assert.False(source.IsCancellationRequested);
            TaskCompletionSource<int> completionSource = create(state, options);
            completionSource.TrySetCanceled(source.Token);
            Assert.False(source.IsCancellationRequested);
            Assert.True(completionSource.Task.IsCanceled);

            TaskCanceledException tce = Assert.Throws<AggregateException>(() => completionSource.Task.Wait()).InnerException as TaskCanceledException;
            Assert.False(tce.CancellationToken.IsCancellationRequested);
        }

        [Theory]
        [MemberData("Constructors")]
        public static void TrySetCanceled_SourceCanceled(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.Cancel();
            Assert.True(source.IsCancellationRequested);
            TaskCompletionSource<int> completionSource = create(state, options);
            completionSource.TrySetCanceled(source.Token);
            Assert.True(source.IsCancellationRequested);

            TaskCanceledException tce = Assert.Throws<AggregateException>(() => completionSource.Task.Wait()).InnerException as TaskCanceledException;
            Assert.True(tce.CancellationToken.IsCancellationRequested);
        }

        [Theory]
        [MemberData("Constructors")]
        public static void Task_Start_Invalid(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            Task<int> task = create(state, options).Task;
            Assert.Throws<InvalidOperationException>(() => task.Start());
        }

        [Theory]
        [MemberData("Constructors")]
        public static void Task_RunSynchronously_Invalid(Func<object, TaskCreationOptions?, TaskCompletionSource<int>> create, object state, TaskCreationOptions? options)
        {
            Task<int> task = create(state, options).Task;
            Assert.Throws<InvalidOperationException>(() => task.RunSynchronously());
        }

        [Theory]
        [InlineData(TaskCreationOptions.DenyChildAttach)]
        [InlineData(TaskCreationOptions.HideScheduler)]
        [InlineData(TaskCreationOptions.LongRunning)]
        [InlineData(TaskCreationOptions.PreferFairness)]
        [InlineData(-1)]
        [InlineData(100)]
        public static void TaskCompletionSource_InvalidOptions(TaskCreationOptions option)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TaskCompletionSource<int>(option));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TaskCompletionSource<int>(new object(), option));
        }

        private static void StartAll(this Task[] tasks)
        {
            foreach (Task task in tasks)
            {
                task.Start();
            }
        }
    }
}
