// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class TaskSchedulerExceptionTests
    {
        [Fact]
        public static void RunTaskSchedulerExceptionTests()
        {
            TaskSchedulerException tse = null;

            tse = new TaskSchedulerException();
            Assert.Null(tse.InnerException); // , "RunTaskSchedulerExceptionTests:  Expected InnerException==null after empty ctor")

            InvalidOperationException ioe = new InvalidOperationException();
            tse = new TaskSchedulerException(ioe);
            Assert.True(tse.InnerException == ioe, "RunTaskSchedulerExceptionTests:  Expected InnerException == ioe passed to ctor(ex)");

            string message = "my exception message";
            tse = new TaskSchedulerException(message);
            Assert.Null(tse.InnerException); // , "RunTaskSchedulerExceptionTests:  Expected InnerException==null after ctor(string)")
            Assert.True(tse.Message.Equals(message), "RunTaskSchedulerExceptionTests:  Expected Message = message passed to ctor(string)");

            tse = new TaskSchedulerException(message, ioe);
            Assert.True(tse.InnerException == ioe, "RunTaskSchedulerExceptionTests:  Expected InnerException == ioe passed to ctor(string, ex)");
            Assert.True(tse.Message.Equals(message), "RunTaskSchedulerExceptionTests:  Expected Message = message passed to ctor(string, ex)");
        }
    }
}
