// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class TaskSchedulerExceptionTests
    {
        [Fact]
        public static void TaskSchedulerException_Constructor_Default()
        {
            TaskSchedulerException tse = new TaskSchedulerException();

            Assert.NotNull(tse);
            Assert.Null(tse.InnerException);
        }

        [Fact]
        public static void TaskSchedulerException_Constructor_Message()
        {
            string message = "message";

            TaskSchedulerException tse = new TaskSchedulerException(message);

            Assert.NotNull(tse);
            Assert.Null(tse.InnerException);
            Assert.Equal(message, tse.Message);
        }

        [Fact]
        public static void TaskSchedulerException_Constructor_InnerException()
        {
            DeliberateTestException inner = new DeliberateTestException();

            TaskSchedulerException tse = new TaskSchedulerException(inner);

            Assert.NotNull(tse);
            Assert.Equal(inner, tse.InnerException);
        }

        [Fact]
        public static void TaskSchedulerException_Constructor_Message_InnerException()
        {
            string message = "message";
            DeliberateTestException inner = new DeliberateTestException();

            TaskSchedulerException tse = new TaskSchedulerException(message, inner);

            Assert.NotNull(tse);
            Assert.Equal(inner, tse.InnerException);
            Assert.Equal(message, tse.Message);
        }
    }
}
