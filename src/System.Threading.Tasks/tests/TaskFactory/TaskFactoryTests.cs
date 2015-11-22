// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public class TaskFactoryTests
    {
        // Expected result from tasks that return values.
        private const int ExpectedResult = 10;

        /// <summary>
        /// Get a task factory created with the given options.
        /// </summary>
        /// Returned data is in the following format:
        ///  1. The task factory
        ///  2. The TaskScheduler used, if any
        ///  3. The TaskCreationOptions used, if any
        ///  4. The CancellationToken used, if any
        ///  5. The TaskContinuationOptions used, if any
        /// <returns>TaskFactoryand creation options</returns>
        public static IEnumerable<object[]> TaskFactory_Data()
        {
            yield return new object[] { new TaskFactory(), null, null, null, null };
            yield return new object[] { new TaskFactory(TaskScheduler.Default), TaskScheduler.Default, null, null, null };
            yield return new object[] { new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None),
                null, TaskCreationOptions.LongRunning, null, TaskContinuationOptions.None };
            yield return new object[] { new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default),
                TaskScheduler.Default, TaskCreationOptions.None, CancellationToken.None, TaskContinuationOptions.None };
            CancellationTokenSource source = new CancellationTokenSource();
            yield return new object[] { new TaskFactory(source.Token), null, null, source.Token, null };
        }

        /// <summary>
        /// Get a task factory created with the given options.
        /// </summary>
        /// Returned data is in the following format:
        ///  1. The task factory
        ///  2. The TaskScheduler used, if any
        ///  3. The TaskCreationOptions used, if any
        ///  4. The CancellationToken used, if any
        ///  5. The TaskContinuationOptions used, if any
        /// <returns>TaskFactoryand creation options</returns>
        public static IEnumerable<object[]> TaskFactory_Int_Data()
        {
            yield return new object[] { new TaskFactory<int>(), null, null, null, null };
            yield return new object[] { new TaskFactory<int>(TaskScheduler.Default), TaskScheduler.Default, null, null, null };
            yield return new object[] { new TaskFactory<int>(TaskCreationOptions.LongRunning, TaskContinuationOptions.None),
                null, TaskCreationOptions.LongRunning, null, TaskContinuationOptions.None };
            yield return new object[] { new TaskFactory<int>(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default),
                TaskScheduler.Default, TaskCreationOptions.None, CancellationToken.None, TaskContinuationOptions.None };
            CancellationTokenSource source = new CancellationTokenSource();
            yield return new object[] { new TaskFactory<int>(source.Token), null, null, source.Token, null };
        }

        #region Test Methods

        // Exercise functionality of TaskFactory and TaskFactory<TResult>
        [Fact]
        public static void RunTaskFactoryTests_Cancellation_Negative()
        {
            CancellationTokenSource cancellationSrc = new CancellationTokenSource();

            //Test constructor that accepts cancellationToken
            cancellationSrc.Cancel();
            TaskFactory tf = new TaskFactory(cancellationSrc.Token);
            var cancelledTask = tf.StartNew(() => { });
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => cancelledTask.Wait());

            // Exercising TF<int>(cancellationToken) with a cancelled token
            cancellationSrc.Cancel();
            TaskFactory<int> tfi = new TaskFactory<int>(cancellationSrc.Token);
            cancelledTask = tfi.StartNew(() => 0);
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => cancelledTask.Wait());
        }

        [Fact]
        public static void RunTaskFactoryExceptionTests()
        {
            // Checking top-level TF exception handling.
            Assert.Throws<ArgumentOutOfRangeException>(
               () => new TaskFactory((TaskCreationOptions)0x40000000, TaskContinuationOptions.None));

            Assert.Throws<ArgumentOutOfRangeException>(
               () => new TaskFactory((TaskCreationOptions)0x100, TaskContinuationOptions.None));

            Assert.Throws<ArgumentOutOfRangeException>(
               () => new TaskFactory(TaskCreationOptions.None, (TaskContinuationOptions)0x40000000));

            Assert.Throws<ArgumentOutOfRangeException>(
               () => new TaskFactory(TaskCreationOptions.None, TaskContinuationOptions.NotOnFaulted));

            // Checking top-level TF<int> exception handling.
            Assert.Throws<ArgumentOutOfRangeException>(
               () => new TaskFactory<int>((TaskCreationOptions)0x40000000, TaskContinuationOptions.None));

            Assert.Throws<ArgumentOutOfRangeException>(
               () => new TaskFactory<int>((TaskCreationOptions)0x100, TaskContinuationOptions.None));

            Assert.Throws<ArgumentOutOfRangeException>(
               () => new TaskFactory<int>(TaskCreationOptions.None, (TaskContinuationOptions)0x40000000));

            Assert.Throws<ArgumentOutOfRangeException>(
               () => new TaskFactory<int>(TaskCreationOptions.None, TaskContinuationOptions.NotOnFaulted));
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_Create_Test(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            Assert.Equal(scheduler, factory.Scheduler);
            Assert.Equal(creation ?? TaskCreationOptions.None, factory.CreationOptions);
            Assert.Equal(continuation ?? TaskContinuationOptions.None, factory.ContinuationOptions);
            Assert.Equal(token ?? CancellationToken.None, factory.CancellationToken);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;
            creation = creation ?? TaskCreationOptions.None;

            Task task = factory.StartNew(() => Assert.Equal(scheduler, TaskScheduler.Current));
            task.Wait();
            Assert.Equal(creation, task.CreationOptions);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew_Token(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;
            creation = creation ?? TaskCreationOptions.None;

            CancellationTokenSource source = new CancellationTokenSource();
            Assert.NotEqual(source.Token, factory.CancellationToken);

            Task task = factory.StartNew(() =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
            }, source.Token);
            task.Wait();
            Assert.Equal(creation, task.CreationOptions);

            source.Cancel();
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
            task = factory.StartNew(() => { /* do nothing */ }, source.Token);
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => task.Wait());
            Assert.True(task.IsCanceled);
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew_Options(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;

            Task task = factory.StartNew(() => Assert.Equal(scheduler, TaskScheduler.Current), TaskCreationOptions.LongRunning);
            task.Wait();
            Assert.Equal(TaskCreationOptions.LongRunning, task.CreationOptions);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew_Token_Options_Scheduler(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            TaskScheduler expected = TaskScheduler.Default;

            CancellationTokenSource source = new CancellationTokenSource();
            Assert.NotEqual(source.Token, factory.CancellationToken);

            Task task = factory.StartNew(() =>
            {
                Assert.Equal(expected, TaskScheduler.Current);
            }, source.Token, TaskCreationOptions.LongRunning, expected);
            task.Wait();
            Assert.Equal(TaskCreationOptions.LongRunning, task.CreationOptions);

            source.Cancel();
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
            task = factory.StartNew(() => { /* do nothing */ }, source.Token, TaskCreationOptions.LongRunning, expected);
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => task.Wait());
            Assert.True(task.IsCanceled);
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew_State(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;
            creation = creation ?? TaskCreationOptions.None;

            object state = new object();

            Task task = factory.StartNew(actual =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
                Assert.Equal(actual, state);
            }, state);
            task.Wait();
            Assert.Equal(creation, task.CreationOptions);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew_State_Token(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;
            creation = creation ?? TaskCreationOptions.None;

            object state = new object();

            CancellationTokenSource source = new CancellationTokenSource();
            Assert.NotEqual(source.Token, factory.CancellationToken);

            Task task = factory.StartNew(actual =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
                Assert.Equal(state, actual);
            }, state, source.Token);
            task.Wait();
            Assert.Equal(creation, task.CreationOptions);

            source.Cancel();
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
            task = factory.StartNew(() => { /* do nothing */ }, source.Token);
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => task.Wait());
            Assert.True(task.IsCanceled);
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew_State_Options(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;

            object state = new object();

            Task task = factory.StartNew(actual =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
                Assert.Equal(actual, state);
            }, state, TaskCreationOptions.LongRunning);
            task.Wait();
            Assert.Equal(TaskCreationOptions.LongRunning, task.CreationOptions);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew_State_Token_Options_Scheduler(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            TaskScheduler expected = TaskScheduler.Default;
            CancellationTokenSource source = new CancellationTokenSource();
            object state = new object();

            Task task = factory.StartNew(actual =>
            {
                Assert.Equal(expected, TaskScheduler.Current);
                Assert.Equal(actual, state);
            }, state, source.Token, TaskCreationOptions.LongRunning, expected);
            task.Wait();
            Assert.Equal(TaskCreationOptions.LongRunning, task.CreationOptions);

            source.Cancel();
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
            task = factory.StartNew(ignore => { /* do nothing */ }, state, source.Token, TaskCreationOptions.LongRunning, expected);
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => task.Wait());
            Assert.True(task.IsCanceled);
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew_Result(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;
            creation = creation ?? TaskCreationOptions.None;

            Task<int> task = factory.StartNew(() =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
                return ExpectedResult;
            });
            task.Wait();
            Assert.Equal(creation, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew_Result_Token(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;
            creation = creation ?? TaskCreationOptions.None;

            CancellationTokenSource source = new CancellationTokenSource();
            Assert.NotEqual(source.Token, factory.CancellationToken);

            Task<int> task = factory.StartNew(() =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
                return ExpectedResult;
            }, source.Token);
            task.Wait();
            Assert.Equal(creation, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);

            source.Cancel();
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
            task = factory.StartNew(() => ExpectedResult, source.Token);
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => task.Wait());
            Assert.True(task.IsCanceled);
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew_Result_Options(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;

            Task<int> task = factory.StartNew(() =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
                return ExpectedResult;
            }, TaskCreationOptions.LongRunning);
            task.Wait();
            Assert.Equal(TaskCreationOptions.LongRunning, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew_Result_Token_Options_Scheduler(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            TaskScheduler expected = TaskScheduler.Default;
            CancellationTokenSource source = new CancellationTokenSource();
            Assert.NotEqual(source.Token, factory.CancellationToken);
            Task<int> task = factory.StartNew(() =>
            {
                Assert.Equal(expected, TaskScheduler.Current);
                return ExpectedResult;
            }, source.Token, TaskCreationOptions.LongRunning, expected);
            task.Wait();
            Assert.Equal(TaskCreationOptions.LongRunning, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);

            source.Cancel();
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
            task = factory.StartNew(() => ExpectedResult, source.Token, TaskCreationOptions.LongRunning, expected);
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => task.Wait());
            Assert.True(task.IsCanceled);
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew_State_Result(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;
            creation = creation ?? TaskCreationOptions.None;

            object state = new object();

            Task<int> task = factory.StartNew(actual =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
                Assert.Equal(actual, state);
                return ExpectedResult;
            }, state);
            task.Wait();
            Assert.Equal(creation, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew_State_Result_Token(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;
            creation = creation ?? TaskCreationOptions.None;

            CancellationTokenSource source = new CancellationTokenSource();
            object state = new object();

            Task<int> task = factory.StartNew(actual =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
                Assert.Equal(actual, state);
                return ExpectedResult;
            }, state, source.Token);
            task.Wait();
            Assert.Equal(creation, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);

            source.Cancel();
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
            task = factory.StartNew(ignore => ExpectedResult, state, source.Token);
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => task.Wait());
            Assert.True(task.IsCanceled);
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew_State_Result_Options(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;

            object state = new object();

            Task<int> task = factory.StartNew(actual =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
                Assert.Equal(actual, state);
                return ExpectedResult;
            }, state, TaskCreationOptions.LongRunning);
            task.Wait();
            Assert.Equal(TaskCreationOptions.LongRunning, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);
        }

        [Theory]
        [MemberData("TaskFactory_Data")]
        public static void TaskFactory_StartNew_State_Result_Token_Options_Scheduler(TaskFactory factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            TaskScheduler expected = TaskScheduler.Default;

            CancellationTokenSource source = new CancellationTokenSource();
            object state = new object();

            Task<int> task = factory.StartNew(actual =>
            {
                Assert.Equal(expected, TaskScheduler.Current);
                Assert.Equal(actual, state);
                return ExpectedResult;
            }, state, source.Token, TaskCreationOptions.LongRunning, expected);
            task.Wait();
            Assert.Equal(TaskCreationOptions.LongRunning, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);

            source.Cancel();
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
            task = factory.StartNew(ignore => ExpectedResult, state, source.Token, TaskCreationOptions.LongRunning, expected);
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => task.Wait());
            Assert.True(task.IsCanceled);
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
        }

        [Theory]
        [MemberData("TaskFactory_Int_Data")]
        public static void TaskFactory_Int_Create_Test(TaskFactory<int> factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            Assert.Equal(scheduler, factory.Scheduler);
            Assert.Equal(creation ?? TaskCreationOptions.None, factory.CreationOptions);
            Assert.Equal(continuation ?? TaskContinuationOptions.None, factory.ContinuationOptions);
            Assert.Equal(token ?? CancellationToken.None, factory.CancellationToken);
        }

        [Theory]
        [MemberData("TaskFactory_Int_Data")]
        public static void TaskFactory_Int_StartNew(TaskFactory<int> factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;
            creation = creation ?? TaskCreationOptions.None;

            Task<int> task = factory.StartNew(() =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
                return ExpectedResult;
            });
            task.Wait();
            Assert.Equal(creation, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);
        }

        [Theory]
        [MemberData("TaskFactory_Int_Data")]
        public static void TaskFactory_Int_StartNew_Token(TaskFactory<int> factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;
            creation = creation ?? TaskCreationOptions.None;

            CancellationTokenSource source = new CancellationTokenSource();
            Assert.NotEqual(source.Token, factory.CancellationToken);

            Task<int> task = factory.StartNew(() =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
                return ExpectedResult;
            }, source.Token);
            task.Wait();
            Assert.Equal(creation, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);

            source.Cancel();
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
            task = factory.StartNew(() => ExpectedResult, source.Token);
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => task.Wait());
            Assert.True(task.IsCanceled);
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
        }

        [Theory]
        [MemberData("TaskFactory_Int_Data")]
        public static void TaskFactory_Int_StartNew_Options(TaskFactory<int> factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;

            Task<int> task = factory.StartNew(() =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
                return ExpectedResult;
            }, TaskCreationOptions.LongRunning);
            task.Wait();
            Assert.Equal(TaskCreationOptions.LongRunning, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);
        }

        [Theory]
        [MemberData("TaskFactory_Int_Data")]
        public static void TaskFactory_Int_StartNew_Token_Options_Scheduler(TaskFactory<int> factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            TaskScheduler expected = TaskScheduler.Default;

            CancellationTokenSource source = new CancellationTokenSource();
            Assert.NotEqual(source.Token, factory.CancellationToken);

            Task<int> task = factory.StartNew(() =>
            {
                Assert.Equal(expected, TaskScheduler.Current);
                return ExpectedResult;
            }, source.Token, TaskCreationOptions.LongRunning, expected);
            task.Wait();
            Assert.Equal(TaskCreationOptions.LongRunning, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);

            source.Cancel();
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
            task = factory.StartNew(() => ExpectedResult, source.Token, TaskCreationOptions.LongRunning, expected);
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => task.Wait());
            Assert.True(task.IsCanceled);
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
        }

        [Theory]
        [MemberData("TaskFactory_Int_Data")]
        public static void TaskFactory_Int_StartNew_State(TaskFactory<int> factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;
            creation = creation ?? TaskCreationOptions.None;

            object state = new object();

            Task<int> task = factory.StartNew(actual =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
                Assert.Equal(actual, state);
                return ExpectedResult;
            }, state);
            task.Wait();
            Assert.Equal(creation, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);
        }

        [Theory]
        [MemberData("TaskFactory_Int_Data")]
        public static void TaskFactory_Int_StartNew_State_Token(TaskFactory<int> factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;
            creation = creation ?? TaskCreationOptions.None;

            CancellationTokenSource source = new CancellationTokenSource();
            object state = new object();

            Task<int> task = factory.StartNew(actual =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
                Assert.Equal(actual, state);
                return ExpectedResult;
            }, state, source.Token);
            task.Wait();
            Assert.Equal(creation, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);

            source.Cancel();
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
            task = factory.StartNew(ignore => ExpectedResult, state, source.Token);
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => task.Wait());
            Assert.True(task.IsCanceled);
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
        }

        [Theory]
        [MemberData("TaskFactory_Int_Data")]
        public static void TaskFactory_Int_StartNew_State_Options(TaskFactory<int> factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            scheduler = scheduler ?? TaskScheduler.Current;

            object state = new object();

            Task<int> task = factory.StartNew(actual =>
            {
                Assert.Equal(scheduler, TaskScheduler.Current);
                Assert.Equal(actual, state);
                return ExpectedResult;
            }, state, TaskCreationOptions.LongRunning);
            task.Wait();
            Assert.Equal(TaskCreationOptions.LongRunning, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);
        }

        [Theory]
        [MemberData("TaskFactory_Int_Data")]
        public static void TaskFactory_Int_StartNew_State_Token_Options_Scheduler(TaskFactory<int> factory, TaskScheduler scheduler, TaskCreationOptions? creation, CancellationToken? token, TaskContinuationOptions? continuation)
        {
            TaskScheduler expected = TaskScheduler.Default;

            CancellationTokenSource source = new CancellationTokenSource();
            object state = new object();

            Task<int> task = factory.StartNew(actual =>
            {
                Assert.Equal(expected, TaskScheduler.Current);
                Assert.Equal(actual, state);
                return ExpectedResult;
            }, state, source.Token, TaskCreationOptions.LongRunning, expected);
            task.Wait();
            Assert.Equal(TaskCreationOptions.LongRunning, task.CreationOptions);
            Assert.Equal(ExpectedResult, task.Result);

            source.Cancel();
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
            task = factory.StartNew(ignore => ExpectedResult, state, source.Token, TaskCreationOptions.LongRunning, expected);
            Functions.AssertThrowsWrapped<TaskCanceledException>(() => task.Wait());
            Assert.True(task.IsCanceled);
            Assert.True(source.Token.IsCancellationRequested);
            Assert.False(factory.CancellationToken.IsCancellationRequested);
        }

        #endregion
    }
}
