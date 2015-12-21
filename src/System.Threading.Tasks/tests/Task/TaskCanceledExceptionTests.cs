// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class TaskCanceledExceptionTests
    {
        [Fact]
        public static void TaskCanceledException_Constructor_Default()
        {
            TaskCanceledException tce = new TaskCanceledException();

            Assert.NotNull(tce);
            Assert.Null(tce.Task);
            Assert.Null(tce.InnerException);
        }

        [Fact]
        public static void TaskCanceledException_Constructor_Message()
        {
            string message = "message";

            TaskCanceledException tce = new TaskCanceledException(message);

            Assert.NotNull(tce);
            Assert.Null(tce.Task);
            Assert.Null(tce.InnerException);
            Assert.Equal(message, tce.Message);
        }

        [Fact]
        public static void TaskCanceledException_Constructor_InnerException()
        {
            string message = "message";
            DeliberateTestException inner = new DeliberateTestException();

            TaskCanceledException tce = new TaskCanceledException(message, inner);

            Assert.NotNull(tce);
            Assert.Null(tce.Task);
            Assert.Equal(inner, tce.InnerException);
            Assert.Equal(message, tce.Message);
        }

        [Fact]
        public static void TaskCanceledException_Task()
        {
            Task task = new Task(() => { });

            TaskCanceledException tce = new TaskCanceledException(task);

            Assert.NotNull(tce);
            Assert.Equal(task, tce.Task);
            Assert.Null(tce.InnerException);
        }
    }
}
