// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public class TaskFactory_CreateTests
    {
        private static readonly TaskCreationOptions[] CreationOptions = (TaskCreationOptions[])Enum.GetValues(typeof(TaskCreationOptions));

        // Valid continuation options for constructor parameters.  Some methods throw if certain options were previously selected.
        private static readonly TaskContinuationOptions[] ContinuationOptions =
            new[] { TaskContinuationOptions.LazyCancellation, TaskContinuationOptions.ExecuteSynchronously }
            .Concat(CreationOptions.Except(new[] { TaskCreationOptions.RunContinuationsAsynchronously }).Cast<TaskContinuationOptions>()).ToArray();

        /// <summary>
        /// Get all combinations of TaskCreationOptions and TaskContinuationOptions.
        /// </summary>
        /// Returned data is in the following format:
        ///  1. TaskCreationOptions
        ///  2. TaskContinuationOptions
        /// <returns>Creation and Continuation options</returns>
        public static IEnumerable<object[]> Creation_And_Continuation_Option_Data()
        {
            foreach (TaskCreationOptions create in CreationOptions)
            {
                foreach (TaskContinuationOptions cont in ContinuationOptions)
                {
                    yield return new object[] { create, cont };
                }
            }
        }

        /// <summary>
        /// Listing of possible TaskSchedulers.
        /// </summary>
        /// A single dimensional array of TaskSchedulers is returned, with the following items:
        ///  1. null - internally this will default to TaskScheduler.Default
        ///  2. TaskScheduler.Default
        ///  3. NonDefaultScheduler - this contains no observable state.
        /// <returns>TaskScheduler</returns>
        public static IEnumerable<object[]> TaskScheduler_Data()
        {
            yield return new object[] { null };
            yield return new object[] { TaskScheduler.Default };
            yield return new object[] { new NonDefaultScheduler() };
        }

        /// <summary>
        /// Get CancellationToken parameters for TaskFactory construction.
        /// </summary>
        /// Returned data is in the following format:
        ///  1. CancellationToken
        ///  2. CancellationTokenSource, if the token is linked to one.
        ///
        /// The following cases are presented:
        ///  a] CancellationToken.None
        ///  b] Unlinked, Uncanceled token
        ///  c] Unlinked, Canceled token
        ///  d] Linked, Uncanceled token with source
        ///  e] Linked, Canceled token with source
        /// <returns>CancellationToken parameters.</returns>
        public static IEnumerable<object[]> CancellationToken_Data()
        {
            yield return new object[] { CancellationToken.None, null };
            yield return new object[] { new CancellationToken(false), null };
            yield return new object[] { new CancellationToken(true), null };
            CancellationTokenSource uncanceled = new CancellationTokenSource();
            yield return new object[] { uncanceled.Token, uncanceled };
            CancellationTokenSource canceled = new CancellationTokenSource();
            canceled.Cancel();
            yield return new object[] { canceled.Token, canceled };
        }

        /// <summary>
        /// Get all combinations of valid parameters.
        /// </summary>
        /// Returned data is in the following format:
        ///  1. The TaskScheduler used, if any
        ///  2. The TaskCreationOptions used, if any
        ///  3. The CancellationToken used, if any
        ///  4. The TaskContinuationOptions used, if any
        ///  5. The CancellationTokenSource, if the token was attached to one.
        ///
        /// TaskScheduler will occasionally be null, and the options may use None
        /// <returns>CreationParameters</returns>
        public static IEnumerable<object[]> All_Parameters_Data()
        {
            foreach (TaskScheduler scheduler in TaskScheduler_Data().Select(s => s[0]))
            {
                foreach (object[] optionPair in Creation_And_Continuation_Option_Data())
                {
                    TaskCreationOptions create = (TaskCreationOptions)optionPair[0];
                    TaskContinuationOptions cont = (TaskContinuationOptions)optionPair[1];

                    foreach (object[] tokenAndSource in CancellationToken_Data())
                    {
                        CancellationToken token = (CancellationToken)tokenAndSource[0];
                        CancellationTokenSource source = (CancellationTokenSource)tokenAndSource[1];

                        yield return new object[] { scheduler, create, token, cont, source };
                    }
                }
            }
        }

        [Fact]
        public static void TaskFactory_Task_Constructor_Default_Test()
        {
            TaskFactory factory = new TaskFactory();

            Assert.Equal(null, factory.Scheduler);
            Assert.Equal(TaskCreationOptions.None, factory.CreationOptions);
            Assert.Equal(TaskContinuationOptions.None, factory.ContinuationOptions);
            Assert.Equal(CancellationToken.None, factory.CancellationToken);
        }

        [Fact]
        public static void TaskFactory_Future_Constructor_Default_Test()
        {
            TaskFactory<object> factory = new TaskFactory<object>();

            Assert.Equal(null, factory.Scheduler);
            Assert.Equal(TaskCreationOptions.None, factory.CreationOptions);
            Assert.Equal(TaskContinuationOptions.None, factory.ContinuationOptions);
            Assert.Equal(CancellationToken.None, factory.CancellationToken);
        }

        [Theory]
        [MemberData(nameof(Creation_And_Continuation_Option_Data))]
        public static void TaskFactory_Task_Constructor_Options_Test(TaskCreationOptions create, TaskContinuationOptions cont)
        {
            TaskFactory factory = new TaskFactory(create, cont);

            Assert.Equal(null, factory.Scheduler);
            Assert.Equal(create, factory.CreationOptions);
            Assert.Equal(cont, factory.ContinuationOptions);
            Assert.Equal(CancellationToken.None, factory.CancellationToken);
        }

        [Theory]
        [MemberData(nameof(Creation_And_Continuation_Option_Data))]
        public static void TaskFactory_Future_Constructor_Options_Test(TaskCreationOptions create, TaskContinuationOptions cont)
        {
            TaskFactory<object> factory = new TaskFactory<object>(create, cont);

            Assert.Equal(null, factory.Scheduler);
            Assert.Equal(create, factory.CreationOptions);
            Assert.Equal(cont, factory.ContinuationOptions);
            Assert.Equal(CancellationToken.None, factory.CancellationToken);
        }

        [Theory]
        [MemberData(nameof(TaskScheduler_Data))]
        public static void TaskFactory_Task_Constructor_Scheduler_Test(TaskScheduler scheduler)
        {
            TaskFactory factory = new TaskFactory(scheduler);

            Assert.Equal(scheduler, factory.Scheduler);
            Assert.Equal(TaskCreationOptions.None, factory.CreationOptions);
            Assert.Equal(TaskContinuationOptions.None, factory.ContinuationOptions);
            Assert.Equal(CancellationToken.None, factory.CancellationToken);
        }

        [Theory]
        [MemberData(nameof(TaskScheduler_Data))]
        public static void TaskFactory_Future_Constructor_Scheduler_Test(TaskScheduler scheduler)
        {
            TaskFactory<object> factory = new TaskFactory<object>(scheduler);

            Assert.Equal(scheduler, factory.Scheduler);
            Assert.Equal(TaskCreationOptions.None, factory.CreationOptions);
            Assert.Equal(TaskContinuationOptions.None, factory.ContinuationOptions);
            Assert.Equal(CancellationToken.None, factory.CancellationToken);
        }

        [Theory]
        [MemberData(nameof(CancellationToken_Data))]
        public static void TaskFactory_Task_Constructor_CancellationToken_Test(CancellationToken token, CancellationTokenSource source)
        {
            TaskFactory factory = new TaskFactory(token);

            Assert.Equal(null, factory.Scheduler);
            Assert.Equal(TaskCreationOptions.None, factory.CreationOptions);
            Assert.Equal(TaskContinuationOptions.None, factory.ContinuationOptions);
            Assert.Equal(token, factory.CancellationToken);

            if (source != null)
            {
                Assert.Equal(source.Token, factory.CancellationToken);

                if (!source.IsCancellationRequested)
                {
                    source.Cancel();
                    Assert.True(factory.CancellationToken.IsCancellationRequested);
                }
            }
        }

        [Theory]
        [MemberData(nameof(CancellationToken_Data))]
        public static void TaskFactory_Future_Constructor_CancellationToken_Test(CancellationToken token, CancellationTokenSource source)
        {
            TaskFactory<object> factory = new TaskFactory<object>(token);

            Assert.Equal(null, factory.Scheduler);
            Assert.Equal(TaskCreationOptions.None, factory.CreationOptions);
            Assert.Equal(TaskContinuationOptions.None, factory.ContinuationOptions);
            Assert.Equal(token, factory.CancellationToken);

            if (source != null)
            {
                Assert.Equal(source.Token, factory.CancellationToken);

                if (!source.IsCancellationRequested)
                {
                    source.Cancel();
                    Assert.True(factory.CancellationToken.IsCancellationRequested);
                }
            }
        }

        [Theory]
        [MemberData(nameof(All_Parameters_Data))]
        public static void TaskFactory_Task_Constructor_All_Test(TaskScheduler scheduler, TaskCreationOptions creation, CancellationToken token, TaskContinuationOptions continuation, CancellationTokenSource source)
        {
            TaskFactory factory = new TaskFactory(token, creation, continuation, scheduler);

            Assert.Equal(scheduler, factory.Scheduler);
            Assert.Equal(creation, factory.CreationOptions);
            Assert.Equal(continuation, factory.ContinuationOptions);
            Assert.Equal(token, factory.CancellationToken);

            if (source != null)
            {
                Assert.Equal(source.Token, factory.CancellationToken);

                if (!source.IsCancellationRequested)
                {
                    source.Cancel();
                    Assert.True(factory.CancellationToken.IsCancellationRequested);
                }
            }
        }

        [Theory]
        [MemberData(nameof(All_Parameters_Data))]
        public static void TaskFactory_Future_Constructor_All_Test(TaskScheduler scheduler, TaskCreationOptions creation, CancellationToken token, TaskContinuationOptions continuation, CancellationTokenSource source)
        {
            TaskFactory<object> factory = new TaskFactory<object>(token, creation, continuation, scheduler);

            Assert.Equal(scheduler, factory.Scheduler);
            Assert.Equal(creation, factory.CreationOptions);
            Assert.Equal(continuation, factory.ContinuationOptions);
            Assert.Equal(token, factory.CancellationToken);

            if (source != null)
            {
                Assert.Equal(source.Token, factory.CancellationToken);

                if (!source.IsCancellationRequested)
                {
                    source.Cancel();
                    Assert.True(factory.CancellationToken.IsCancellationRequested);
                }
            }
        }

        [Theory]
        [InlineData(TaskContinuationOptions.LazyCancellation)]
        [InlineData(0x80)]
        public static void TaskFactory_Task_TaskCreationOptions_Invalid_Test(TaskCreationOptions option)
        {
            Assert.Throws<ArgumentOutOfRangeException>("creationOptions", () => new TaskFactory(option, TaskContinuationOptions.None));
            Assert.Throws<ArgumentOutOfRangeException>("creationOptions", () => new TaskFactory(CancellationToken.None, option, TaskContinuationOptions.None, TaskScheduler.Default));
        }

        [Theory]
        [InlineData(TaskContinuationOptions.LazyCancellation)]
        [InlineData(0x80)]
        public static void TaskFactory_Future_TaskCreationOptions_Invalid_Test(TaskCreationOptions option)
        {
            Assert.Throws<ArgumentOutOfRangeException>("creationOptions", () => new TaskFactory<int>(option, TaskContinuationOptions.None));
            Assert.Throws<ArgumentOutOfRangeException>("creationOptions", () => new TaskFactory<int>(CancellationToken.None, option, TaskContinuationOptions.None, TaskScheduler.Default));
        }

        [Theory]
        [InlineData(TaskContinuationOptions.NotOnCanceled)]
        [InlineData(TaskContinuationOptions.NotOnFaulted)]
        [InlineData(TaskContinuationOptions.NotOnRanToCompletion)]
        [InlineData(TaskContinuationOptions.OnlyOnCanceled)]
        [InlineData(TaskContinuationOptions.OnlyOnFaulted)]
        [InlineData(TaskContinuationOptions.OnlyOnRanToCompletion)]
        [InlineData(TaskContinuationOptions.RunContinuationsAsynchronously)]
        [InlineData(TaskContinuationOptions.LongRunning | TaskContinuationOptions.ExecuteSynchronously)]
        [InlineData(0x80)]
        public static void TaskFactory_Task_TaskContinuationOptions_Invalid_Test(TaskContinuationOptions option)
        {
            Assert.Throws<ArgumentOutOfRangeException>("continuationOptions", () => new TaskFactory(TaskCreationOptions.None, option));
            Assert.Throws<ArgumentOutOfRangeException>("continuationOptions", () => new TaskFactory(CancellationToken.None, TaskCreationOptions.None, option, TaskScheduler.Default));
        }

        [Theory]
        [InlineData(TaskContinuationOptions.NotOnCanceled)]
        [InlineData(TaskContinuationOptions.NotOnFaulted)]
        [InlineData(TaskContinuationOptions.NotOnRanToCompletion)]
        [InlineData(TaskContinuationOptions.OnlyOnCanceled)]
        [InlineData(TaskContinuationOptions.OnlyOnFaulted)]
        [InlineData(TaskContinuationOptions.OnlyOnRanToCompletion)]
        [InlineData(TaskContinuationOptions.RunContinuationsAsynchronously)]
        [InlineData(TaskContinuationOptions.LongRunning | TaskContinuationOptions.ExecuteSynchronously)]
        [InlineData(0x80)]
        public static void TaskFactory_Future_TaskContinuationOptions_Invalid_Test(TaskContinuationOptions option)
        {
            Assert.Throws<ArgumentOutOfRangeException>("continuationOptions", () => new TaskFactory<int>(TaskCreationOptions.None, option));
            Assert.Throws<ArgumentOutOfRangeException>("continuationOptions", () => new TaskFactory<int>(CancellationToken.None, TaskCreationOptions.None, option, TaskScheduler.Default));
        }

        private class NonDefaultScheduler : TaskScheduler
        {
            protected override IEnumerable<Task> GetScheduledTasks()
            {
                return Enumerable.Empty<Task>();
            }

            protected override void QueueTask(Task task)
            {
                Task.Run(() => TryExecuteTask(task));
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                if (taskWasPreviouslyQueued)
                {
                    return false;
                }

                return TryExecuteTask(task);
            }
        }
    }
}
