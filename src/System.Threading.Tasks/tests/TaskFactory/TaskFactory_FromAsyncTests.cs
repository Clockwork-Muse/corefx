// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Linq;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public class TaskFactory_FromAsyncTests
    {
        [Fact]
        public static void FromAsync_IAsyncResult()
        {
            Task completed = CompletedTask();
            bool callbackRan = false;

            Task t = new TaskFactory().FromAsync(completed, result => AssertResultAndMark(completed, result, ref callbackRan));
            t.Wait();

            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
        }

        [Theory]
        [InlineData(TaskCreationOptions.AttachedToParent)]
        [InlineData(TaskCreationOptions.None)]
        public static void FromAsync_IAsyncResult_TaskCreationOptions(TaskCreationOptions options)
        {
            Task completed = CompletedTask();
            bool callbackRan = false;

            Task t = new TaskFactory().FromAsync(completed, result => AssertResultAndMark(completed, result, ref callbackRan), options);
            t.Wait();

            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Equal(options, t.CreationOptions);
        }

        [Fact]
        public static void Task_FromAsync()
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object state = new object();

            Task t = new TaskFactory().FromAsync(
                (callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan),
                result => AssertResultAndMark(completed, result, ref callbackRan), state);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Same(state, t.AsyncState);
        }

        [Fact]
        public static void Task_FromAsync_OneArg()
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object arg1 = new object();
            object state = new object();

            Task t = new TaskFactory().FromAsync(
                (a1, callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan, arg1, a1),
                result => AssertResultAndMark(completed, result, ref callbackRan), arg1, state);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Same(state, t.AsyncState);
        }

        [Fact]
        public static void Task_FromAsync_TwoArg()
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object arg1 = new object();
            object arg2 = new object();
            object state = new object();

            Task t = new TaskFactory().FromAsync(
                (a1, a2, callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan, arg1, a1, arg2, a2),
                result => AssertResultAndMark(completed, result, ref callbackRan), arg1, arg2, state);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Same(state, t.AsyncState);
        }

        [Fact]
        public static void Task_FromAsync_ThreeArg()
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object arg1 = new object();
            object arg2 = new object();
            object arg3 = new object();
            object state = new object();

            Task t = new TaskFactory().FromAsync(
                (a1, a2, a3, callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan, arg1, a1, arg2, a2, arg3, a3),
                result => AssertResultAndMark(completed, result, ref callbackRan), arg1, arg2, arg3, state);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Same(state, t.AsyncState);
        }

        [Fact]
        public static void Task_Result_FromAsync_ThreeArg()
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object arg1 = new object();
            object arg2 = new object();
            object arg3 = new object();
            object state = new object();
            object value = new object();

            Task<object> t = Task<object>.Factory.FromAsync(
                (a1, a2, a3, callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan, arg1, a1, arg2, a2, arg3, a3),
                result => AssertResultAndReturn(completed, result, ref callbackRan, value), arg1, arg2, arg3, state);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Same(state, t.AsyncState);
            Assert.Same(value, t.Result);
        }

        [Fact]
        public static void Task_Result_FromAsync_TwoArg()
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object arg1 = new object();
            object arg2 = new object();
            object state = new object();
            object value = new object();

            Task<object> t = Task<object>.Factory.FromAsync(
                (a1, a2, callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan, arg1, a1, arg2, a2),
                result => AssertResultAndReturn(completed, result, ref callbackRan, value), arg1, arg2, state);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Same(state, t.AsyncState);
            Assert.Same(value, t.Result);
        }

        [Fact]
        public static void Task_Result_FromAsync_OneArg()
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object arg1 = new object();
            object state = new object();
            object value = new object();

            Task<object> t = Task<object>.Factory.FromAsync(
                (a1, callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan, arg1, a1),
                result => AssertResultAndReturn(completed, result, ref callbackRan, value), arg1, state);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Same(state, t.AsyncState);
            Assert.Same(value, t.Result);
        }

        [Fact]
        public static void Task_Result_FromAsync()
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object state = new object();
            object value = new object();

            Task<object> t = Task<object>.Factory.FromAsync(
                (callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan),
                result => AssertResultAndReturn(completed, result, ref callbackRan, value), state);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Same(state, t.AsyncState);
            Assert.Same(value, t.Result);
        }

        [Fact]
        public static void FromAsync_Result_ThreeArg()
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object arg1 = new object();
            object arg2 = new object();
            object arg3 = new object();
            object state = new object();
            object value = new object();

            Task<object> t = new TaskFactory().FromAsync(
                (a1, a2, a3, callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan, arg1, a1, arg2, a2, arg3, a3),
                result => AssertResultAndReturn(completed, result, ref callbackRan, value), arg1, arg2, arg3, state);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Same(state, t.AsyncState);
            Assert.Same(value, t.Result);
        }

        [Theory]
        [InlineData(TaskCreationOptions.AttachedToParent)]
        [InlineData(TaskCreationOptions.None)]
        public static void FromAsync_Result_ThreeArg_TaskCreationOptions(TaskCreationOptions options)
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object arg1 = new object();
            object arg2 = new object();
            object arg3 = new object();
            object state = new object();
            object value = new object();

            Task<object> t = new TaskFactory().FromAsync(
                (a1, a2, a3, callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan, arg1, a1, arg2, a2, arg3, a3),
                result => AssertResultAndReturn(completed, result, ref callbackRan, value), arg1, arg2, arg3, state, options);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Equal(options, t.CreationOptions);
            Assert.Same(state, t.AsyncState);
            Assert.Same(value, t.Result);
        }

        [Fact]
        public static void FromAsync_Result_TwoArg()
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object arg1 = new object();
            object arg2 = new object();
            object state = new object();
            object value = new object();

            Task<object> t = new TaskFactory().FromAsync(
                (a1, a2, callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan, arg1, a1, arg2, a2),
                result => AssertResultAndReturn(completed, result, ref callbackRan, value), arg1, arg2, state);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Same(state, t.AsyncState);
            Assert.Same(value, t.Result);
        }

        [Theory]
        [InlineData(TaskCreationOptions.AttachedToParent)]
        [InlineData(TaskCreationOptions.None)]
        public static void FromAsync_Result_TwoArg_TaskCreationOption(TaskCreationOptions options)
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object arg1 = new object();
            object arg2 = new object();
            object state = new object();
            object value = new object();

            Task<object> t = new TaskFactory().FromAsync(
                (a1, a2, callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan, arg1, a1, arg2, a2),
                result => AssertResultAndReturn(completed, result, ref callbackRan, value), arg1, arg2, state, options);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Equal(options, t.CreationOptions);
            Assert.Same(state, t.AsyncState);
            Assert.Same(value, t.Result);
        }

        [Fact]
        public static void FromAsync_Result_OneArg()
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object arg1 = new object();
            object state = new object();
            object value = new object();

            Task<object> t = new TaskFactory().FromAsync(
                (a1, callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan, arg1, a1),
                result => AssertResultAndReturn(completed, result, ref callbackRan, value), arg1, state);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Same(state, t.AsyncState);
            Assert.Same(value, t.Result);
        }

        [Theory]
        [InlineData(TaskCreationOptions.AttachedToParent)]
        [InlineData(TaskCreationOptions.None)]
        public static void FromAsync_Result_OneArg_TaskCreationOption(TaskCreationOptions options)
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object arg1 = new object();
            object arg2 = new object();
            object arg3 = new object();
            object state = new object();
            object value = new object();

            Task<object> t = new TaskFactory().FromAsync(
                (a1, a2, a3, callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan, arg1, a1, arg2, a2, arg3, a3),
                result => AssertResultAndReturn(completed, result, ref callbackRan, value), arg1, arg2, arg3, state, options);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Equal(options, t.CreationOptions);
            Assert.Same(state, t.AsyncState);
            Assert.Same(value, t.Result);
        }

        [Fact]
        public static void FromAsync_Result()
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object state = new object();
            object value = new object();

            Task<object> t = new TaskFactory().FromAsync(
                (callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan),
                result => AssertResultAndReturn(completed, result, ref callbackRan, value), state);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Same(state, t.AsyncState);
            Assert.Same(value, t.Result);
        }

        [Theory]
        [InlineData(TaskCreationOptions.AttachedToParent)]
        [InlineData(TaskCreationOptions.None)]
        public static void FromAsync_Result_TaskCreationOption(TaskCreationOptions options)
        {
            Task completed = CompletedTask();
            bool asyncRan = false;
            bool callbackRan = false;
            object state = new object();
            object value = new object();

            Task<object> t = new TaskFactory().FromAsync(
                (callback, asyncState) => AssertStateAndMark(completed, callback, state, asyncState, ref asyncRan),
                result => AssertResultAndReturn(completed, result, ref callbackRan, value), state, options);
            t.Wait();

            Assert.True(asyncRan);
            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Equal(options, t.CreationOptions);
            Assert.Same(state, t.AsyncState);
            Assert.Same(value, t.Result);
        }

        [Fact]
        public static void Task_Result_FromAsync_IAsyncResult()
        {
            Task completed = CompletedTask();
            object value = new object();
            bool callbackRan = false;

            Task<object> t = Task<object>.Factory.FromAsync(completed, result => AssertResultAndReturn(completed, result, ref callbackRan, value));
            t.Wait();

            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Same(value, t.Result);
        }

        [Fact]
        public static void FromAsync_Result_IAsyncResult()
        {
            Task completed = CompletedTask();
            object value = new object();
            bool callbackRan = false;

            Task<object> t = new TaskFactory().FromAsync(completed, result => AssertResultAndReturn(completed, result, ref callbackRan, value));
            t.Wait();

            Assert.True(callbackRan);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
            Assert.Same(value, t.Result);
        }

        [Fact]
        public async static void FromAsync_CallbackException()
        {
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync(CompletedTask(), result => { throw new DeliberateTestException(); }));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync(CompletedTask(), result => { throw new DeliberateTestException(); }, TaskCreationOptions.None));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync(CompletedTask(), result => { throw new DeliberateTestException(); }, TaskCreationOptions.None, TaskScheduler.Default));

            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync((callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "state"));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync((callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "state", TaskCreationOptions.None));

            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync((arg1, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "state"));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync((arg1, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "state", TaskCreationOptions.None));

            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync((arg1, arg2, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "state"));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync((arg1, arg2, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "state", TaskCreationOptions.None));

            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "arg3", "state"));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "arg3", "state", TaskCreationOptions.None));

            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync<string>(CompletedTask(), result => { throw new DeliberateTestException(); }));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync<string>(CompletedTask(), result => { throw new DeliberateTestException(); }, TaskCreationOptions.None));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync<string>(CompletedTask(), result => { throw new DeliberateTestException(); }, TaskCreationOptions.None, TaskScheduler.Default));

            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync<string>((callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "state"));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync<string>((callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "state", TaskCreationOptions.None));

            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync<string, string>((arg1, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "state"));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync<string, string>((arg1, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "state", TaskCreationOptions.None));

            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync<string, string, string>((arg1, arg2, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "state"));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync<string, string, string>((arg1, arg2, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "state", TaskCreationOptions.None));

            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync<string, string, string, string>((arg1, arg2, arg3, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "arg3", "state"));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory().FromAsync<string, string, string, string>((arg1, arg2, arg3, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "arg3", "state", TaskCreationOptions.None));

            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory<string>().FromAsync(CompletedTask(), result => { throw new DeliberateTestException(); }));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory<string>().FromAsync(CompletedTask(), result => { throw new DeliberateTestException(); }, TaskCreationOptions.None));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory<string>().FromAsync(CompletedTask(), result => { throw new DeliberateTestException(); }, TaskCreationOptions.None, TaskScheduler.Default));

            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory<string>().FromAsync((callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "state"));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory<string>().FromAsync((callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "state", TaskCreationOptions.None));

            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory<string>().FromAsync((arg1, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "state"));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory<string>().FromAsync((arg1, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "state", TaskCreationOptions.None));

            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory<string>().FromAsync((arg1, arg2, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "state"));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory<string>().FromAsync((arg1, arg2, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "state", TaskCreationOptions.None));

            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory<string>().FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "arg3", "state"));
            await Assert.ThrowsAsync<DeliberateTestException>(
                () => new TaskFactory<string>().FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "arg3", "state", TaskCreationOptions.None));
        }

        [Fact]
        public static void FromAsync_BeginException()
        {
            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((callback, state) => { throw new DeliberateTestException(); }, result => { }, "state"); });
            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((callback, state) => { throw new DeliberateTestException(); }, result => { }, "state", TaskCreationOptions.None); });

            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((arg1, callback, state) => { throw new DeliberateTestException(); }, result => { }, "arg1", "state"); });
            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((arg1, callback, state) => { throw new DeliberateTestException(); }, result => { }, "arg1", "state", TaskCreationOptions.None); });

            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((arg1, arg2, callback, state) => { throw new DeliberateTestException(); }, result => { }, "arg1", "arg2", "state"); });
            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((arg1, arg2, callback, state) => { throw new DeliberateTestException(); }, result => { }, "arg1", "arg2", "state", TaskCreationOptions.None); });

            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((arg1, arg2, arg3, callback, state) => { throw new DeliberateTestException(); }, result => { }, "arg1", "arg2", "arg3", "state"); });
            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((arg1, arg2, arg3, callback, state) => { throw new DeliberateTestException(); }, result => { }, "arg1", "arg2", "arg3", "state", TaskCreationOptions.None); });

            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "state"); });
            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "state", TaskCreationOptions.None); });

            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((arg1, callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "arg1", "state"); });
            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((arg1, callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "arg1", "state", TaskCreationOptions.None); });

            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((arg1, arg2, callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "arg1", "arg2", "state"); });
            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((arg1, arg2, callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "arg1", "arg2", "state", TaskCreationOptions.None); });

            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((arg1, arg2, arg3, callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "arg1", "arg2", "arg3", "state"); });
            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory().FromAsync((arg1, arg2, arg3, callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "arg1", "arg2", "arg3", "state", TaskCreationOptions.None); });

            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory<string>().FromAsync((callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "state"); });
            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory<string>().FromAsync((callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "state", TaskCreationOptions.None); });

            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory<string>().FromAsync((arg1, callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "arg1", "state"); });
            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory<string>().FromAsync((arg1, callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "arg1", "state", TaskCreationOptions.None); });

            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory<string>().FromAsync((arg1, arg2, callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "arg1", "arg2", "state"); });
            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory<string>().FromAsync((arg1, arg2, callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "arg1", "arg2", "state", TaskCreationOptions.None); });

            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory<string>().FromAsync((arg1, arg2, arg3, callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "arg1", "arg2", "arg3", "state"); });
            Assert.Throws<DeliberateTestException>(
                () => { new TaskFactory<string>().FromAsync((arg1, arg2, arg3, callback, state) => { throw new DeliberateTestException(); }, result => string.Empty, "arg1", "arg2", "arg3", "state", TaskCreationOptions.None); });
        }

        [Fact]
        public async static void FromAsync_Canceled()
        {
            Task t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync(CompletedTask(), result => { throw new OperationCanceledException(); }));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync(CompletedTask(), result => { throw new OperationCanceledException(); }, TaskCreationOptions.None));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync(CompletedTask(), result => { throw new OperationCanceledException(); }, TaskCreationOptions.None, TaskScheduler.Default));
            Assert.True(t.IsCanceled);

            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync((callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "state"));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync((callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "state", TaskCreationOptions.None));
            Assert.True(t.IsCanceled);

            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync((arg1, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "state"));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync((arg1, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "state", TaskCreationOptions.None));
            Assert.True(t.IsCanceled);

            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync((arg1, arg2, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "arg2", "state"));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync((arg1, arg2, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "arg2", "state", TaskCreationOptions.None));
            Assert.True(t.IsCanceled);

            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "arg2", "arg3", "state"));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "arg2", "arg3", "state", TaskCreationOptions.None));
            Assert.True(t.IsCanceled);

            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync<string>(CompletedTask(), result => { throw new OperationCanceledException(); }));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync<string>(CompletedTask(), result => { throw new OperationCanceledException(); }, TaskCreationOptions.None));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync<string>(CompletedTask(), result => { throw new OperationCanceledException(); }, TaskCreationOptions.None, TaskScheduler.Default));
            Assert.True(t.IsCanceled);

            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync<string>((callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "state"));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync<string>((callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "state", TaskCreationOptions.None));
            Assert.True(t.IsCanceled);

            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync<string, string>((arg1, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "state"));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync<string, string>((arg1, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "state", TaskCreationOptions.None));
            Assert.True(t.IsCanceled);

            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync<string, string, string>((arg1, arg2, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "arg2", "state"));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync<string, string, string>((arg1, arg2, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "arg2", "state", TaskCreationOptions.None));
            Assert.True(t.IsCanceled);

            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync<string, string, string, string>((arg1, arg2, arg3, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "arg2", "arg3", "state"));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory().FromAsync<string, string, string, string>((arg1, arg2, arg3, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "arg2", "arg3", "state", TaskCreationOptions.None));
            Assert.True(t.IsCanceled);

            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory<string>().FromAsync(CompletedTask(), result => { throw new OperationCanceledException(); }));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory<string>().FromAsync(CompletedTask(), result => { throw new OperationCanceledException(); }, TaskCreationOptions.None));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory<string>().FromAsync(CompletedTask(), result => { throw new OperationCanceledException(); }, TaskCreationOptions.None, TaskScheduler.Default));
            Assert.True(t.IsCanceled);

            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory<string>().FromAsync((callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "state"));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory<string>().FromAsync((callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "state", TaskCreationOptions.None));
            Assert.True(t.IsCanceled);

            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory<string>().FromAsync((arg1, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "state"));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory<string>().FromAsync((arg1, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "state", TaskCreationOptions.None));
            Assert.True(t.IsCanceled);

            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory<string>().FromAsync((arg1, arg2, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "arg2", "state"));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory<string>().FromAsync((arg1, arg2, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "arg2", "state", TaskCreationOptions.None));
            Assert.True(t.IsCanceled);

            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory<string>().FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "arg2", "arg3", "state"));
            Assert.True(t.IsCanceled);
            t = await Functions.AssertThrowsAsync<OperationCanceledException>(
                () => new TaskFactory<string>().FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), result => { throw new OperationCanceledException(); }, "arg1", "arg2", "arg3", "state", TaskCreationOptions.None));
            Assert.True(t.IsCanceled);
        }

        [Fact]
        public static void FromAsync_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
               () => { new TaskFactory().FromAsync(Task.CompletedTask, null); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync(null, result => { }); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync(null, result => { }, TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync(Task.CompletedTask, null, TaskCreationOptions.None); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync(null, result => { }, TaskCreationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync(Task.CompletedTask, null, TaskCreationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync(Task.CompletedTask, result => { }, TaskCreationOptions.None, null); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync((callback, state) => completion(callback), null, "state"); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync(null, result => { }, "state"); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync((callback, state) => completion(callback), null, "state", TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync(null, result => { }, (object)"state", TaskCreationOptions.None); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync((arg1, callback, state) => completion(callback), null, "arg1", "state"); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync(null, result => { }, "arg1", "state"); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync((arg1, callback, state) => completion(callback), null, "arg1", "state", TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync(null, result => { }, "arg1", (object)"state", TaskCreationOptions.None); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync((arg1, arg2, callback, state) => completion(callback), null, "arg1", "arg2", "state"); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync(null, result => { }, "arg1", "arg2", "state"); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync((arg1, arg2, callback, state) => completion(callback), null, "arg1", "arg2", "state", TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync(null, result => { }, "arg1", "arg2", (object)"state", TaskCreationOptions.None); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), null, "arg1", "arg2", "arg3", "state"); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync(null, result => { }, "arg1", "arg2", "arg3", "state"); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), null, "arg1", "arg2", "arg3", "state", TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync(null, result => { }, "arg1", "arg2", "arg3", "state", TaskCreationOptions.None); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string>(null, result => string.Empty); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string>(Task.CompletedTask, null); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string>(null, result => string.Empty, TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string>(Task.CompletedTask, null, TaskCreationOptions.None); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string>(null, result => string.Empty, TaskCreationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string>(Task.CompletedTask, null, TaskCreationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string>(Task.CompletedTask, result => string.Empty, TaskCreationOptions.None, null); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string>((callback, state) => completion(callback), null, "state"); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string>(null, result => string.Empty, "state"); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string>((callback, state) => completion(callback), null, "state", TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string>(null, result => string.Empty, "state", TaskCreationOptions.None); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string, string>((arg1, callback, state) => completion(callback), null, "arg1", "state"); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string, string>(null, result => string.Empty, "arg1", "state"); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string, string>((arg1, callback, state) => completion(callback), null, "arg1", "state", TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string, string>(null, result => string.Empty, "arg1", "state", TaskCreationOptions.None); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string, string, string>((arg1, arg2, callback, state) => completion(callback), null, "arg1", "arg2", "state"); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string, string, string>(null, result => string.Empty, "arg1", "arg2", "state"); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string, string, string>((arg1, arg2, callback, state) => completion(callback), null, "arg1", "arg2", "state", TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string, string, string>(null, result => string.Empty, "arg1", "arg2", "state", TaskCreationOptions.None); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string, string, string, string>((arg1, arg2, arg3, callback, state) => completion(callback), null, "arg1", "arg2", "arg3", "state"); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string, string, string, string>(null, result => string.Empty, "arg1", "arg2", "arg3", "state"); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string, string, string, string>((arg1, arg2, arg3, callback, state) => completion(callback), null, "arg1", "arg2", "arg3", "state", TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory().FromAsync<string, string, string, string>(null, result => string.Empty, "arg1", "arg2", "arg3", "state", TaskCreationOptions.None); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync(null, result => string.Empty); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync(Task.CompletedTask, null); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync(null, result => string.Empty, TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync(Task.CompletedTask, null, TaskCreationOptions.None); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync(null, result => string.Empty, TaskCreationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync(Task.CompletedTask, null, TaskCreationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync(Task.CompletedTask, result => string.Empty, TaskCreationOptions.None, null); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync((callback, state) => completion(callback), null, "state"); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync(null, result => string.Empty, "state"); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync((callback, state) => completion(callback), null, "state", TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync(null, result => string.Empty, (object)"state", TaskCreationOptions.None); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync((arg1, callback, state) => completion(callback), null, "arg1", "state"); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync(null, result => string.Empty, "arg1", "state"); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync((arg1, callback, state) => completion(callback), null, "arg1", "state", TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync(null, result => string.Empty, "arg1", (object)"state", TaskCreationOptions.None); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync((arg1, arg2, callback, state) => completion(callback), null, "arg1", "arg2", "state"); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync(null, result => string.Empty, "arg1", "arg2", "state"); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync((arg1, arg2, callback, state) => completion(callback), null, "arg1", "arg2", "state", TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync(null, result => string.Empty, "arg1", "arg2", (object)"state", TaskCreationOptions.None); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), null, "arg1", "arg2", "arg3", "state"); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync(null, result => string.Empty, "arg1", "arg2", "arg3", "state"); });

            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), null, "arg1", "arg2", "arg3", "state", TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>(
                () => { new TaskFactory<string>().FromAsync(null, result => string.Empty, "arg1", "arg2", "arg3", "state", TaskCreationOptions.None); });
        }

        [Theory]
        [InlineData(TaskCreationOptions.LongRunning)]
        [InlineData(TaskCreationOptions.PreferFairness)]
        public static void TaskFactory_FromAsync_ArgumentOutOfRange(TaskCreationOptions options)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
               () => { new TaskFactory(options, TaskContinuationOptions.None).FromAsync((callback, state) => completion(callback), result => { }, "state"); });
            Assert.Throws<ArgumentOutOfRangeException>(
               () => { new TaskFactory(options, TaskContinuationOptions.None).FromAsync((arg1, callback, state) => completion(callback), result => { }, "arg1", "state"); });
            Assert.Throws<ArgumentOutOfRangeException>(
               () => { new TaskFactory(options, TaskContinuationOptions.None).FromAsync((arg1, arg2, callback, state) => completion(callback), result => { }, "arg1", "arg2", "state"); });
            Assert.Throws<ArgumentOutOfRangeException>(
               () => { new TaskFactory(options, TaskContinuationOptions.None).FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), result => { }, "arg1", "arg2", "arg3", "state"); });

            Assert.Throws<ArgumentOutOfRangeException>(
                () => { new TaskFactory<string>(options, TaskContinuationOptions.None).FromAsync((callback, state) => completion(callback), result => string.Empty, "state"); });
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { new TaskFactory<string>(options, TaskContinuationOptions.None).FromAsync((arg1, callback, state) => completion(callback), result => string.Empty, "arg1", "state"); });
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { new TaskFactory<string>(options, TaskContinuationOptions.None).FromAsync((arg1, arg2, callback, state) => completion(callback), result => string.Empty, "arg1", "arg2", "state"); });
            Assert.Throws<ArgumentOutOfRangeException>(
                () => { new TaskFactory<string>(options, TaskContinuationOptions.None).FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), result => string.Empty, "arg1", "arg2", "arg3", "state"); });
        }

        [Fact]
        public static void FromAsync_CompletesOnChildException()
        {
            Func<Task>[] create = new Func<Task>[]
            {
                () => new TaskFactory().FromAsync(CompletedTask(), result => { throw new DeliberateTestException(); }, TaskCreationOptions.AttachedToParent),
                () => new TaskFactory().FromAsync(CompletedTask(), result => { throw new DeliberateTestException(); }, TaskCreationOptions.AttachedToParent, TaskScheduler.Default),
                () => new TaskFactory().FromAsync((callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "state", TaskCreationOptions.AttachedToParent),
                () => new TaskFactory().FromAsync((arg1, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "state", TaskCreationOptions.AttachedToParent),
                () => new TaskFactory().FromAsync((arg1, arg2, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "state", TaskCreationOptions.AttachedToParent),
                () => new TaskFactory().FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "arg3", "state", TaskCreationOptions.AttachedToParent),
                () => new TaskFactory().FromAsync<string>(CompletedTask(), result => { throw new DeliberateTestException(); }, TaskCreationOptions.AttachedToParent),
                () => new TaskFactory().FromAsync<string>(CompletedTask(), result => { throw new DeliberateTestException(); }, TaskCreationOptions.AttachedToParent, TaskScheduler.Default),
                () => new TaskFactory().FromAsync<string>((callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "state", TaskCreationOptions.AttachedToParent),
                () => new TaskFactory().FromAsync<string, string>((arg1, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "state", TaskCreationOptions.AttachedToParent),
                () => new TaskFactory().FromAsync<string, string, string>((arg1, arg2, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "state", TaskCreationOptions.AttachedToParent),
                () => new TaskFactory().FromAsync<string, string, string, string>((arg1, arg2, arg3, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "arg3", "state", TaskCreationOptions.AttachedToParent),
                () => new TaskFactory<string>().FromAsync(CompletedTask(), result => { throw new DeliberateTestException(); }, TaskCreationOptions.AttachedToParent),
                () => new TaskFactory<string>().FromAsync(CompletedTask(), result => { throw new DeliberateTestException(); }, TaskCreationOptions.AttachedToParent, TaskScheduler.Default),
                () => new TaskFactory<string>().FromAsync((callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "state", TaskCreationOptions.AttachedToParent),
                () => new TaskFactory<string>().FromAsync((arg1, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "state", TaskCreationOptions.AttachedToParent),
                () => new TaskFactory<string>().FromAsync((arg1, arg2, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "state", TaskCreationOptions.AttachedToParent),
                () => new TaskFactory<string>().FromAsync((arg1, arg2, arg3, callback, state) => completion(callback), result => { throw new DeliberateTestException(); }, "arg1", "arg2", "arg3", "state", TaskCreationOptions.AttachedToParent),
            };

            Task[] tasks = new Task[create.Length];

            Task outer = new TaskFactory().StartNew(() =>
            {
                for (int i = 0; i < create.Length; i++)
                {
                    tasks[i] = create[i]();
                }
            });

            Debug.WriteLine("FromAsync_CompletesOnChildException: Waiting on task w/ faulted FromAsync() calls.  If we hang, there is a problem");
            AggregateException ae = Assert.Throws<AggregateException>(() => outer.Wait());
            Debug.WriteLine("FromAsync_CompletesOnChildException: Completed, no problem");

            Assert.True(outer.IsFaulted);
            Assert.Equal(TaskStatus.Faulted, outer.Status);
            Assert.Equal(create.Length, ae.InnerExceptions.Count);
            Assert.All(tasks, task =>
            {
                Assert.True(task.IsFaulted);
                Assert.Equal(TaskStatus.Faulted, task.Status);
            });
        }

        [ActiveIssue("https://github.com/dotnet/coreclr/issues/7892")] // BinaryCompatibility reverting FromAsync to .NET 4 behavior, causing invokesCallback=false to fail
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FromAsync_CompletedSynchronouslyIAsyncResult_CompletesSynchronously(bool invokesCallback)
        {
            Task t = Task.Factory.FromAsync((callback, state) =>
            {
                var ar = new SynchronouslyCompletedAsyncResult { AsyncState = state };
                if (invokesCallback) callback(ar);
                return ar;
            }, iar => { }, null);
            Assert.Equal(TaskStatus.RanToCompletion, t.Status);
        }

        private sealed class SynchronouslyCompletedAsyncResult : IAsyncResult
        {
            public object AsyncState { get; internal set; }
            public bool CompletedSynchronously => true;
            public bool IsCompleted => true;
            public WaitHandle AsyncWaitHandle { get { throw new NotImplementedException(); } }
        }

        /// <summary>
        /// Get a unique completed task.
        /// </summary>
        /// Task.CompletedTask returns a cached instance;
        ///  this version returns a unique one on each call.
        /// <returns>A unique completed task</returns>
        private static Task CompletedTask()
        {
            Task completed = Task.Run(() => { /* do nothing */ });
            completed.Wait();
            return completed;
        }

        private static Func<AsyncCallback, IAsyncResult> completion =
           callback =>
           {
               Task completed = CompletedTask();
               callback(completed);
               return completed;
           };

        private static void AssertResultAndMark(Task completed, IAsyncResult result, ref bool callbackRan)
        {
            Assert.Same(completed, result);
            callbackRan = true;
        }

        private static T AssertResultAndReturn<T>(Task completed, IAsyncResult result, ref bool callbackRan, T value)
        {
            Assert.Same(completed, result);
            callbackRan = true;
            return value;
        }

        private static IAsyncResult AssertStateAndMark(IAsyncResult result, AsyncCallback callback, object state, object asyncState, ref bool asyncRan, params object[] args)
        {
            if (args.Length % 2 != 0)
            {
                throw new ArgumentException("Arguments need to be in expected/actual pairs");
            }
            Assert.Same(state, asyncState);
            // Elements at even indices are expected, odd indices actual.
            Assert.Equal(args.Where((e, i) => i % 2 == 0), args.Where((e, i) => i % 2 == 1));
            callback(result);
            asyncRan = true;
            return result;
        }
    }
}
