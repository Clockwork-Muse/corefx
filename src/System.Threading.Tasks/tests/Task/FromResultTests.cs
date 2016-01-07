// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class FromResultTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(42)]
        [InlineData(-42)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public static void FromResult_ValueType(int value)
        {
            Task<int> task = Task.FromResult(value);

            // Task already completed
            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
            Assert.Null(task.Exception);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.Equal(value, task.Result);

            // Completed promise, returns immediately
            task.Wait();

            Assert.Equal(TaskCreationOptions.None, task.CreationOptions);
            Assert.Null(task.AsyncState);

            Assert.Equal(task, task);
            Assert.NotEqual(task, Task.FromResult(~value));
            // Due to tasks having a unique id, and common use cases, framework caching of result tasks is unlikely
            Assert.NotEqual(task, Task.FromResult(value));
        }

        public static IEnumerable<object[]> FromResult_ReferenceType_Data()
        {
            yield return new object[] { Tuple.Create(0) };
            yield return new object[] { (object)0 };
            yield return new object[] { "0" };
            yield return new object[] { new object() };
            yield return new object[] { null };
        }

        [Theory]
        [MemberData("FromResult_ReferenceType_Data")]
        public static void FromResult_ReferenceType(object result)
        {
            Task<object> task = Task.FromResult(result);

            // Task already completed
            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.False(task.IsFaulted);
            Assert.Null(task.Exception);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.Equal(result, task.Result);

            // Completed promise, returns immediately
            task.Wait();

            Assert.Equal(TaskCreationOptions.None, task.CreationOptions);
            Assert.Null(task.AsyncState);

            Assert.Equal(task, task);
            Assert.NotEqual(task, Task.FromResult(new object()));
            Assert.NotEqual(task, Task.FromResult((object)null));
            // Due to tasks having a unique id, and common use cases, framework caching of result tasks is unlikely
            Assert.NotEqual(task, Task.FromResult(result));
        }
    }
}
