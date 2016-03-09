// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class TaskFactory_StartNewTests
    {
        /// <summary>
        /// Get all StartNew/StartNew-Future calls not taking a scheduler.
        /// </summary>
        /// Returned data is in the following format:
        ///  1. The label describing the StartNew call (to be ignored).
        ///  2. The StartNew call, presented as a Func taking the TaskFactory to run off and an Action, and return the resulting task.
        ///
        /// This set of data is intended to be used to verify the use of the default scheduler when the factory specifies none.
        /// Because StartNew cannot specify a null scheduler, those calls specifying a scheduler are omitted here.
        /// <returns>Labeled StartNew calls</returns>
        public static IEnumerable<object[]> TaskFactory_Task_Scheduler_Data()
        {
            yield return new object[] { "StartNew(action)", (Func<TaskFactory, Action, Task>)((TaskFactory factory, Action capture) => factory.StartNew(capture)) };
            yield return new object[] { "StartNew(action, token)", (Func<TaskFactory, Action, Task>)((TaskFactory factory, Action capture) => factory.StartNew(capture, new CancellationToken())) };
            yield return new object[] { "StartNew(action, options)", (Func<TaskFactory, Action, Task>)((TaskFactory factory, Action capture) => factory.StartNew(capture, TaskCreationOptions.None)) };
            yield return new object[] { "StartNew(action, state)", (Func<TaskFactory, Action, Task>)((TaskFactory factory, Action capture) => factory.StartNew(ignore => capture(), new object())) };
            yield return new object[] { "StartNew(action, state, token)", (Func<TaskFactory, Action, Task>)((TaskFactory factory, Action capture) => factory.StartNew(ignore => capture(), new object(), new CancellationToken())) };
            yield return new object[] { "StartNew(action, state, options)", (Func<TaskFactory, Action, Task>)((TaskFactory factory, Action capture) => factory.StartNew(ignore => capture(), new object(), TaskCreationOptions.None)) };

            yield return new object[] { "StartNew<object>(func)", (Func<TaskFactory, Action, Task<object>>)((TaskFactory factory, Action capture) => factory.StartNew(() => { capture(); return new object(); })) };
            yield return new object[] { "StartNew<object>(func, token)", (Func<TaskFactory, Action, Task<object>>)((TaskFactory factory, Action capture) => factory.StartNew(() => { capture(); return new object(); }, new CancellationToken())) };
            yield return new object[] { "StartNew<object>(func, options)", (Func<TaskFactory, Action, Task<object>>)((TaskFactory factory, Action capture) => factory.StartNew(() => { capture(); return new object(); }, TaskCreationOptions.None)) };
            yield return new object[] { "StartNew<object>(funcn, state)", (Func<TaskFactory, Action, Task<object>>)((TaskFactory factory, Action capture) => factory.StartNew(ignore => { capture(); return new object(); }, new object())) };
            yield return new object[] { "StartNew<object>(func, state, token)", (Func<TaskFactory, Action, Task<object>>)((TaskFactory factory, Action capture) => factory.StartNew(ignore => { capture(); return new object(); }, new object(), new CancellationToken())) };
            yield return new object[] { "StartNew<object>(func, state, options)", (Func<TaskFactory, Action, Task<object>>)((TaskFactory factory, Action capture) => factory.StartNew(ignore => { capture(); return new object(); }, new object(), TaskCreationOptions.None)) };
        }

        /// <summary>
        /// Get all StartNew-Future calls not taking a scheduler.
        /// </summary>
        /// Returned data is in the following format:
        ///  1. The label describing the StartNew call (to be ignored).
        ///  2. The StartNew call, presented as a Func taking the TaskFactory-Future to run off and an Action, and return the resulting task.
        ///
        /// This set of data is intended to be used to verify the use of the default scheduler when the factory specifies none.
        /// Because StartNew cannot specify a null scheduler, those calls specifying a scheduler are omitted here.
        /// <returns>Labeled StartNew calls</returns>
        public static IEnumerable<object[]> TaskFactory_Future_Scheduler_Data()
        {
            yield return new object[] { "StartNew<object>(func)", (Func<TaskFactory<object>, Action, Task<object>>)((TaskFactory<object> factory, Action capture) => factory.StartNew(() => { capture(); return new object(); })) };
            yield return new object[] { "StartNew<object>(func, token)", (Func<TaskFactory<object>, Action, Task<object>>)((TaskFactory<object> factory, Action capture) => factory.StartNew(() => { capture(); return new object(); }, new CancellationToken())) };
            yield return new object[] { "StartNew<object>(func, options)", (Func<TaskFactory<object>, Action, Task<object>>)((TaskFactory<object> factory, Action capture) => factory.StartNew(() => { capture(); return new object(); }, TaskCreationOptions.None)) };
            yield return new object[] { "StartNew<object>(funcn, state)", (Func<TaskFactory<object>, Action, Task<object>>)((TaskFactory<object> factory, Action capture) => factory.StartNew(ignore => { capture(); return new object(); }, new object())) };
            yield return new object[] { "StartNew<object>(func, state, token)", (Func<TaskFactory<object>, Action, Task<object>>)((TaskFactory<object> factory, Action capture) => factory.StartNew(ignore => { capture(); return new object(); }, new object(), new CancellationToken())) };
            yield return new object[] { "StartNew<object>(func, state, options)", (Func<TaskFactory<object>, Action, Task<object>>)((TaskFactory<object> factory, Action capture) => factory.StartNew(ignore => { capture(); return new object(); }, new object(), TaskCreationOptions.None)) };
        }

        [Theory]
        [MemberData(nameof(TaskFactory_Task_Scheduler_Data))]
        public static void TaskFactory_Task_Scheduler_Default(string label, Func<TaskFactory, Action, Task> start)
        {
            TaskFactory factory = new TaskFactory();

            // Ideally there would be a better way to observe the use of the default scheduler, but we have to rely on capturing.
            TaskScheduler captured = null;
            Action capture = () => { captured = TaskScheduler.Current; };

            Task started = start(factory, capture);
            Spin.UntilOrTimeout(() => captured != null);
            Assert.Equal(TaskScheduler.Default, captured);
        }

        [Theory]
        [MemberData(nameof(TaskFactory_Future_Scheduler_Data))]
        public static void TaskFactory_Future_Scheduler_Default(string label, Func<TaskFactory<object>, Action, Task> start)
        {
            TaskFactory<object> factory = new TaskFactory<object>();

            // Ideally there would be a better way to observe the use of the default scheduler, but we have to rely on capturing.
            TaskScheduler captured = null;
            Action capture = () => { captured = TaskScheduler.Current; };

            Task started = start(factory, capture);
            Spin.UntilOrTimeout(() => captured != null);
            Assert.Equal(TaskScheduler.Default, captured);
        }

        [Fact]
        public static void TaskFactory_StartNew_ArgumentNull()
        {
            // Specifically enclosing in brackets to avoid the await, and make it clear the exception is thrown on method invocation.
            Assert.Throws<ArgumentNullException>("action", () => { new TaskFactory().StartNew(null); });
            Assert.Throws<ArgumentNullException>("action", () => { new TaskFactory().StartNew(null, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>("action", () => { new TaskFactory().StartNew(null, TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>("action", () => { new TaskFactory().StartNew(null, new object()); });
            Assert.Throws<ArgumentNullException>("action", () => { new TaskFactory().StartNew(null, new object(), new CancellationToken()); });
            Assert.Throws<ArgumentNullException>("action", () => { new TaskFactory().StartNew(null, new object(), TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>("action", () => { new TaskFactory().StartNew(null, new CancellationToken(), TaskCreationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>("scheduler", () => { new TaskFactory().StartNew(() => { }, new CancellationToken(), TaskCreationOptions.None, null); });
            Assert.Throws<ArgumentNullException>("action", () => { new TaskFactory().StartNew(null, new object(), new CancellationToken(), TaskCreationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>("scheduler", () => { new TaskFactory().StartNew(i => { }, new object(), new CancellationToken(), TaskCreationOptions.None, null); });

            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory().StartNew((Func<object>)null); });
            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory().StartNew((Func<object>)null, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory().StartNew((Func<object>)null, TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory().StartNew((Func<object, int>)null, new object()); });
            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory().StartNew((Func<object, int>)null, new object(), new CancellationToken()); });
            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory().StartNew((Func<object, int>)null, new object(), TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory().StartNew((Func<object>)null, new CancellationToken(), TaskCreationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>("scheduler", () => { new TaskFactory().StartNew(() => 0, new CancellationToken(), TaskCreationOptions.None, null); });
            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory().StartNew((Func<object, int>)null, new object(), new CancellationToken(), TaskCreationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>("scheduler", () => { new TaskFactory().StartNew(i => 0, new object(), new CancellationToken(), TaskCreationOptions.None, null); });

            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory<object>().StartNew(null); });
            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory<object>().StartNew(null, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory<object>().StartNew(null, TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory<object>().StartNew(null, new object()); });
            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory<object>().StartNew(null, new object(), new CancellationToken()); });
            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory<object>().StartNew(null, new object(), TaskCreationOptions.None); });
            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory<object>().StartNew(null, new CancellationToken(), TaskCreationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>("scheduler", () => { new TaskFactory<object>().StartNew(() => 0, new CancellationToken(), TaskCreationOptions.None, null); });
            Assert.Throws<ArgumentNullException>("function", () => { new TaskFactory<object>().StartNew(null, new object(), new CancellationToken(), TaskCreationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>("scheduler", () => { new TaskFactory<object>().StartNew(i => 0, new object(), new CancellationToken(), TaskCreationOptions.None, null); });
        }
    }
}
