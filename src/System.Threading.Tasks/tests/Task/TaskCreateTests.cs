// Licensed to the .NET Foundation under one or more agreements. 
// The .NET Foundation licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information. 


// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// Test class using UnitTestDriver that ensures all the public ctor of Task, Future and
// promise are properly working
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Collections.Generic;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class TaskCreateTests
    {
        /// <summary>
        /// Get FutureT tasks to start
        /// </summary>
        /// Format of returned data is:
        /// 1. String label (ignored)
        /// 2. Task
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> FutureT_Data()
        {
            yield return new object[] { "Task<bool>", new Task<bool>(() => true) };
            yield return new object[] { "Task<bool>|CancellationToken", new Task<bool>(() => true, new CancellationTokenSource().Token) };
            yield return new object[] { "Task<bool>|TaskCreationOptions", new Task<bool>(() => true, TaskCreationOptions.None) };
            yield return new object[] { "Task<bool>|CancellationToken|TaskCreationOptions",
                 new Task<bool>(() => true, new CancellationTokenSource().Token, TaskCreationOptions.None) };
            yield return new object[] { "Task<bool>|state", new Task<bool>(state => { Assert.Equal(1, state); return true; }, 1) };
            yield return new object[] { "Task<bool>|state|CancellationToken",
                new Task<bool>(state => { Assert.Equal(1, state); return true; }, 1, new CancellationTokenSource().Token) };
            yield return new object[] { "Task<bool>|state|TaskCreationOptions",
                new Task<bool>(state => { Assert.Equal(1, state); return true; }, 1, TaskCreationOptions.None) };
            yield return new object[] { "Task<bool>|state|CancellationToken|TaskCreationOptions",
                new Task<bool>(state => { Assert.Equal(1, state); return true; }, 1, new CancellationTokenSource().Token, TaskCreationOptions.None) };
        }

        /// <summary>
        /// Get Tasks to start
        /// </summary>
        /// Format of returned data is:
        /// 1. String label (ignored)
        /// 2. Func to create a task that modifies a flag
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> Task_Data()
        {
            yield return new object[] { "Task", (Func<Flag, Task>)(flag => new Task(() => flag.Trip())) };
            yield return new object[] { "Task|CancellationToken", (Func<Flag, Task>)(flag => new Task(() => flag.Trip(), new CancellationTokenSource().Token)) };
            yield return new object[] { "Task|TaskCreationOptions", (Func<Flag, Task>)(flag => new Task(() => flag.Trip(), TaskCreationOptions.None)) };
            yield return new object[] { "Task|CancellationToken|TaskCreationOptions",
                (Func<Flag, Task>)( flag => new Task(() => flag.Trip(), new CancellationTokenSource().Token, TaskCreationOptions.None)) };
            yield return new object[] { "Task|state", (Func<Flag, Task>)(flag => new Task(state => { Assert.Equal(1, state); flag.Trip(); }, 1)) };
            yield return new object[] { "Task|state|CancellationToken",
                (Func<Flag, Task>)(flag => new Task(state => { Assert.Equal(1, state); flag.Trip(); }, 1, new CancellationTokenSource().Token)) };
            yield return new object[] { "Task|state|TaskCreationOptions",
                (Func<Flag, Task>)(flag => new Task(state => { Assert.Equal(1, state); flag.Trip(); }, 1, TaskCreationOptions.None)) };
            yield return new object[] { "Task|state|CancellationToken|TaskCreationOptions",
                (Func<Flag, Task>)(flag => new Task(state => { Assert.Equal(1, state); flag.Trip(); }, 1, new CancellationTokenSource().Token, TaskCreationOptions.None)) };
        }

        /// <summary>
        /// Get FutureT tasks submitted via StartNew
        /// </summary>
        /// Format of returned data is:
        /// 1. String label (ignored)
        /// 2. Task (already running)
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> FutureT_Started_Data()
        {
            yield return new object[] { "Task<bool>", Task<bool>.Factory.StartNew(() => true) };
            yield return new object[] { "Task<bool>|CancellationToken", Task<bool>.Factory.StartNew(() => true, new CancellationTokenSource().Token) };
            yield return new object[] { "Task<bool>|TaskCreationOptions", Task<bool>.Factory.StartNew(() => true, TaskCreationOptions.None) };
            yield return new object[] { "Task<bool>|CancellationToken|TaskCreationOptions",
                Task<bool>.Factory.StartNew(() => true, new CancellationTokenSource().Token, TaskCreationOptions.None, TaskScheduler.Default) };
            yield return new object[] { "Task<bool>|state", Task<bool>.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1) };
            yield return new object[] { "Task<bool>|state|CancellationToken",
                Task<bool>.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1, new CancellationTokenSource().Token) };
            yield return new object[] { "Task<bool>|state|TaskCreationOptions",
                Task<bool>.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1, TaskCreationOptions.None) };
            yield return new object[] { "Task<bool>|state|CancellationToken|TaskCreationOptions",
                Task<bool>.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1, new CancellationTokenSource().Token, TaskCreationOptions.None, TaskScheduler.Default) };

            yield return new object[] { "StartNew<bool>", Task.Factory.StartNew(() => true) };
            yield return new object[] { "StartNew<bool>|CancellationToken", Task.Factory.StartNew(() => true, new CancellationTokenSource().Token) };
            yield return new object[] { "StartNew<bool>|TaskCreationOptions", Task.Factory.StartNew(() => true, TaskCreationOptions.None) };
            yield return new object[] { "StartNew<bool>|CancellationToken|TaskCreationOptions|TaskScheduler",
                Task.Factory.StartNew(() => true, new CancellationTokenSource().Token, TaskCreationOptions.None, TaskScheduler.Default) };
            yield return new object[] { "StartNew<bool>|state", Task.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1) };
            yield return new object[] { "StartNew<bool>|state|CancellationToken",
                Task.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1, new CancellationTokenSource().Token) };
            yield return new object[] { "StartNew<bool>|state|TaskCreationOptions",
                Task.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1, TaskCreationOptions.None) };
            yield return new object[] { "StartNew<bool>|state|CancellationToken|TaskCreationOptions|TaskScheduler",
                Task.Factory.StartNew(state => { Assert.Equal(1, state); return true; }, 1, new CancellationTokenSource().Token, TaskCreationOptions.None, TaskScheduler.Default) };
        }

        /// <summary>
        /// Get Tasks to be submitted via StartNew
        /// </summary>
        /// Format of returned data is:
        /// 1. String label (ignored)
        /// 2. Func to create a running task that modifies a flag
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> Task_Started_Data()
        {
            yield return new object[] { "Task", (Func<Flag, Task>)(flag => Task.Factory.StartNew(() => flag.Trip())) };
            yield return new object[] { "Task|CancellationToken", (Func<Flag, Task>)(flag => Task.Factory.StartNew(() => flag.Trip(), new CancellationTokenSource().Token)) };
            yield return new object[] { "Task|TaskCreationOptions", (Func<Flag, Task>)(flag => Task.Factory.StartNew(() => flag.Trip(), TaskCreationOptions.None)) };
            yield return new object[] { "Task|CancellationToken|TaskCreationOptions|TaskScheduler",
                (Func<Flag, Task>)( flag => Task.Factory.StartNew(() => flag.Trip(), new CancellationTokenSource().Token, TaskCreationOptions.None,TaskScheduler.Default)) };
            yield return new object[] { "Task|state", (Func<Flag, Task>)(flag => Task.Factory.StartNew(state => { Assert.Equal(1, state); flag.Trip(); }, 1)) };
            yield return new object[] { "Task|state|CancellationToken",
                (Func<Flag, Task>)(flag => Task.Factory.StartNew(state => { Assert.Equal(1, state); flag.Trip(); }, 1, new CancellationTokenSource().Token)) };
            yield return new object[] { "Task|state|TaskCreationOptions",
                (Func<Flag, Task>)(flag => Task.Factory.StartNew(state => { Assert.Equal(1, state); flag.Trip(); }, 1, TaskCreationOptions.None)) };
            yield return new object[] { "Task|state|CancellationToken|TaskCreationOptions|TaskScheduler",
                (Func<Flag, Task>)(flag => Task.Factory.StartNew(state => { Assert.Equal(1, state); flag.Trip(); }, 1, new CancellationTokenSource().Token, TaskCreationOptions.None, TaskScheduler.Default)) };
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_Create_Test(string label, Func<Flag, Task> create)
        {
            Future_Create_Test(label, create(new Flag()));
        }

        [Theory]
        [MemberData("FutureT_Data")]
        public static void Future_Create_Test<T>(string label, T task) where T : Task
        {
            Assert.NotNull(task);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsCompleted);
            Assert.False(task.IsFaulted);
            Assert.Equal(TaskStatus.Created, task.Status);
            // Required so Xunit doesn't complain during dispose
            task.RunSynchronously();
        }

        [Fact]
        public static void TaskCancellable_Test()
        {
            AssertCancellable(token => new Task<bool>(() => true, token));
            AssertCancellable(token => new Task<bool>(() => true, token, TaskCreationOptions.None));
            AssertCancellable(token => new Task<bool>(ignored => true, new object(), token, TaskCreationOptions.None));
            AssertCancellable(token => new Task(() => { }, token));
            AssertCancellable(token => new Task(() => { }, token, TaskCreationOptions.None));
            AssertCancellable(token => new Task(ignored => { }, new object(), token, TaskCreationOptions.None));
        }

        private static void AssertCancellable<T>(Func<CancellationToken, T> create) where T : Task
        {
            CancellationTokenSource source = new CancellationTokenSource();
            T task = create(source.Token);
            source.Cancel();

            Assert.True(task.IsCanceled);
            Assert.Equal(TaskStatus.Canceled, task.Status);
        }

        [Fact]
        public static void Create_Promise_Test()
        {
            Assert.NotNull(new TaskCompletionSource<object>(new object(), TaskCreationOptions.None).Task);
            Assert.NotNull(new TaskCompletionSource<object>(TaskCreationOptions.None).Task);
            Assert.NotNull(new TaskCompletionSource<object>(new object()).Task);
            Assert.NotNull(new TaskCompletionSource<object>().Task);
        }

        [Theory]
        [MemberData("FutureT_Started_Data")]
        public async static void Future_StartNew_Test(string label, Task<bool> task)
        {
            Assert.True(await task);
        }

        [Theory]
        [MemberData("Task_Started_Data")]
        public static void Task_StartNew_Test(string label, Func<Flag, Task> create)
        {
            AssertCompletes(create);
        }

        private static void AssertCompletes(Func<Flag, Task> create)
        {
            Flag flag = new Flag();
            Task task = create(flag);
            task.Wait();
            Assert.True(flag.IsTripped);
        }

        [Theory]
        [MemberData("FutureT_Data")]
        public async static void Future_Start_Test(string label, Task<bool> future)
        {
            Assert.True(await Start(future));
        }

        [Theory]
        [MemberData("FutureT_Data")]
        public async static void Future_Start_Scheduler_Test(string label, Task<bool> future)
        {
            Assert.True(await Start(future, TaskScheduler.Default));
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_Start_Test(string label, Func<Flag, Task> create)
        {
            AssertCompletes(flag => Start(create(flag)));
        }

        [Theory]
        [MemberData("Task_Data")]
        public static void Task_Start_Scheduler_Test(string label, Func<Flag, Task> create)
        {
            AssertCompletes(flag => Start(create(flag), TaskScheduler.Default));
        }

        [Fact]
        public static void RunRefactoringTests_NegativeTests()
        {
            int temp = 0;
            Task<int> f = Task.Factory.StartNew<int>((object i) => { return (int)i; }, 1, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Current);
            Task t;
            temp = f.Result;
            if (temp != 1)
            {
                Assert.True(false, string.Format("RunRefactoringTests - Task.Factory.StartNew<int>(Func<object, int>, object, CT, options, TaskScheduler).    > FAILED.  Delegate failed to execute."));
            }

            f = new TaskCompletionSource<int>().Task;
            try
            {
                f.Start();
                Assert.True(false, string.Format("RunRefactoringTests - TaskCompletionSource<int>.Task (should throw exception):    > FAILED.  No exception thrown."));
            }
            catch (Exception)
            {
                //Assert.True(false, string.Format("    > caught exception: {0}", e.Message));
            }

            t = new Task(delegate { temp = 100; });
            t.Start();
            try
            {
                t.Start();
                Assert.True(false, string.Format("RunRefactoringTests - Restarting Task:    > FAILED.  No exception thrown, when there should be."));
            }
            catch (Exception)
            {
                //Assert.True(false, string.Format("    > caught exception: {0}", e.Message));
            }

            // If we don't do this, the asynchronous setting of temp=100 in the delegate could
            // screw up some tests below.
            t.Wait();

            try
            {
                t = new Task(delegate { temp = 100; }, (TaskCreationOptions)10000);
                Assert.True(false, string.Format("RunRefactoringTests - Illegal Options CTor Task:    > FAILED.  No exception thrown, when there should be."));
            }
            catch (Exception) { }

            try
            {
                t = new Task(null);
                Assert.True(false, string.Format("RunRefactoringTests - Task ctor w/ null action:    > FAILED.  No exception thrown."));
            }
            catch (Exception) { }

            try
            {
                t = Task.Factory.StartNew(null);
                Assert.True(false, string.Format("RunRefactoringTests - Task.Factory.StartNew() w/ Null Action:    > FAILED.  No exception thrown."));
            }
            catch (Exception) { }

            t = new Task(delegate { });
            Task t2 = t.ContinueWith(delegate { });
            try
            {
                t2.Start();
                Assert.True(false, string.Format("RunRefactoringTests - Task.Start() on Continuation Task:    > FAILED.  No exception thrown."));
            }
            catch (Exception) { }

            t = new Task(delegate { });
            try
            {
                t.Start(null);
                Assert.True(false, string.Format("RunRefactoringTests - Task.Start() with null taskScheduler:    > FAILED.  No exception thrown."));
            }
            catch (Exception) { }

            t = Task.Factory.StartNew(delegate { });
            try
            {
                t = Task.Factory.StartNew(delegate { }, CancellationToken.None, TaskCreationOptions.None, (TaskScheduler)null);
                Assert.True(false, string.Format("RunRefactoringTests - Task.Factory.StartNew() with null taskScheduler:    > FAILED.  No exception thrown."));
            }
            catch (Exception) { }
        }

        // Test overloads for Task<T> ctor, Task<T>.Factory.StartNew that accept a TaskCreationOptions param
        [Fact]
        public static void TestTaskTConstruction_tco()
        {
            for (int i = 0; i < 2; i++)
            {
                bool useCtor = (i == 0);
                for (int j = 0; j < 2; j++)
                {
                    bool useObj = (j == 0);
                    object refObj = new object();
                    for (int k = 0; k < 2; k++)
                    {
                        bool useLongRunning = (k == 0);
                        Task<int> f1;
                        TaskCreationOptions tco = useLongRunning ? TaskCreationOptions.LongRunning : TaskCreationOptions.None;

                        if (useCtor)
                        {
                            if (useObj)
                            {
                                f1 = new Task<int>(obj => 42, refObj, tco);
                            }
                            else
                            {
                                f1 = new Task<int>(() => 42, tco);
                            }
                            f1.Start();
                        }
                        else
                        {
                            if (useObj)
                            {
                                f1 = Task<int>.Factory.StartNew(obj => 42, refObj, tco);
                            }
                            else
                            {
                                f1 = Task<int>.Factory.StartNew(() => 42, tco);
                            }
                        }

                        Exception ex = null;
                        int result = 0;
                        try
                        {
                            result = f1.Result;
                        }
                        catch (Exception e)
                        {
                            ex = e;
                        }

                        object asyncState = ((IAsyncResult)f1).AsyncState;

                        Assert.True((ex == null), "TestTaskTConstruction_tco:  Did not expect an exception");
                        Assert.True(f1.CreationOptions == tco, "TestTaskTConstruction_tco:  Mis-matched TaskCreationOptions");
                        Assert.True(result == 42, "TestTaskTConstruction_tco:  Expected valid result");
                        Assert.True(useObj || (asyncState == null), "TestTaskTConstruction_tco:  Expected non-null AsyncState only if object overload was used");
                        Assert.True((!useObj) || (asyncState == refObj), "TestTaskTConstruction_tco:  Wrong AsyncState value returned");
                    }
                }
            }
        }

        [Fact]
        public static void RunBasicFutureTest_Negative()
        {
            //
            // future basic functionality tests
            //

            // Test exceptional conditions
            Assert.Throws<ArgumentNullException>(
               () => { new Task<int>((Func<int>)null); });
            Assert.Throws<ArgumentNullException>(
               () => { new Task<int>((Func<object, int>)null, new object()); });
            Assert.Throws<ArgumentNullException>(
               () => { Task<int>.Factory.StartNew((Func<int>)null); });
            Assert.Throws<ArgumentNullException>(
               () => { Task<int>.Factory.StartNew((Func<int>)null, CancellationToken.None, TaskCreationOptions.None, (TaskScheduler)null); });
            Assert.Throws<ArgumentNullException>(
               () => { Task<int>.Factory.StartNew((Func<object, int>)null, new object()); });
            Assert.Throws<ArgumentNullException>(
               () => { Task<int>.Factory.StartNew((obj) => 42, new object(), CancellationToken.None, TaskCreationOptions.None, (TaskScheduler)null); });
        }

        private static T Start<T>(T task) where T : Task
        {
            task.Start();
            return task;
        }

        private static T Start<T>(T task, TaskScheduler scheduler) where T : Task
        {
            task.Start(scheduler);
            return task;
        }

        [Fact]
        public static void StartOnContinueInvalid_Tests()
        {
            Task t = new Task(() => { }).ContinueWith(ignore => { });
            Assert.Throws<InvalidOperationException>(() => t.Start());
        }

        [Fact]
        public static void MultipleStartInvalid_Tests()
        {
            Task t = new Task(() => { });
            t.Start();
            Assert.Throws<InvalidOperationException>(() => t.Start());
        }

        [Fact]
        public static void StartOnPromiseInvalid_Tests()
        {
            TaskCompletionSource<object> completionSource = new TaskCompletionSource<object>();
            Task<object> task = completionSource.Task;
            Assert.Throws<InvalidOperationException>(() => task.Start());
        }

        [Fact]
        public async static void ArgumentNullException_Tests()
        {
            Assert.Throws<ArgumentNullException>(() => { new Task(null); });
            Assert.Throws<ArgumentNullException>(() => { new Task<object>(null); });
            await Assert.ThrowsAsync<ArgumentNullException>(() => Task.Factory.StartNew(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Task<object>.Factory.StartNew(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Task.Factory.StartNew<object>(null));

            // Task scheduler cannot be null
            Assert.Throws<ArgumentNullException>(() => new Task(() => { }).Start(null));
            Assert.Throws<ArgumentNullException>(() => new Task<object>(() => new object()).Start(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Task.Factory.StartNew(() => { }, new CancellationToken(), TaskCreationOptions.None, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Task<object>.Factory.StartNew(() => new object(), new CancellationToken(), TaskCreationOptions.None, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Task.Factory.StartNew<object>(() => new object(), new CancellationToken(), TaskCreationOptions.None, null));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(32)]
        [InlineData(128)]
        public async static void InvalidTaskCreationOption_Tests(int option)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { new Task(() => { }, (TaskCreationOptions)option); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new Task<object>(() => new object(), (TaskCreationOptions)option); });
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Task.Factory.StartNew(() => { }, (TaskCreationOptions)option));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Task<object>.Factory.StartNew(() => new object(), (TaskCreationOptions)option));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Task.Factory.StartNew<object>(() => new object(), (TaskCreationOptions)option));
        }
    }
}
