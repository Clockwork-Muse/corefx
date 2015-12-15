// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class TaskCanceledExceptionTests
    {
        [Fact]
        public static void RunTaskCanceledExceptionTests()
        {
            TaskCanceledException tce = null;

            // Test empty constructor
            tce = new TaskCanceledException();
            Assert.Null(tce.Task); // , "RunTaskCanceledExceptionTests:  Expected null Task prop after empty ctor")
            Assert.Null(tce.InnerException); // , "RunTaskCanceledExceptionTests:  Expected null InnerException prop after empty ctor")

            string message = "my exception message";
            tce = new TaskCanceledException(message);
            Assert.True(tce.Message.Equals(message), "RunTaskCanceledExceptionTests:  Message != string passed to ctor(string)");
            Assert.Null(tce.Task); // , "RunTaskCanceledExceptionTests:  Expected null Task prop after ctor(string)")
            Assert.Null(tce.InnerException); // , "RunTaskCanceledExceptionTests:  Expected null InnerException prop after ctor(string)")

            Task t1 = Task.Factory.StartNew(() => { });
            tce = new TaskCanceledException(t1);
            Assert.True(tce.Task == t1, "RunTaskCanceledExceptionTests:  Task != task passed to ctor(Task)");
            Assert.Null(tce.InnerException); // , "RunTaskCanceledExceptionTests:  Expected null InnerException prop after ctor(Task)")

            InvalidOperationException ioe = new InvalidOperationException();
            tce = new TaskCanceledException(message, ioe);
            Assert.True(tce.Message.Equals(message), "RunTaskCanceledExceptionTests:  Message != string passed to ctor(string, exception)");
            Assert.Null(tce.Task); // , "RunTaskCanceledExceptionTests:  Expected null Task prop after ctor(string, exception)")
            Assert.True(tce.InnerException == ioe, "RunTaskCanceledExceptionTests:  InnerException != exception passed to ctor(string, exception)");
        }
    }
}
