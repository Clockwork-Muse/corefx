// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;

namespace System.Threading.Tasks.Tests
{
    /// <summary>
    /// TaskScheduler that keeps references to scheduled tasks.
    /// </summary>
    /// While the scheduling task id is retained, and tasks retain schedule order by task,
    /// inter-task schedule timing and ordering is deliberately not maintained, nor does it allow prediction of
    /// task start time or running thread id.
    ///
    /// Tasks started during normal execution (ie, not child tasks) have a scheduling task id of 0.
    ///
    /// After capture, tasks are pushed down and scheduled to run on the default thread-pool scheduler.
    internal class CapturingTaskScheduler : TaskScheduler
    {
        private readonly IDictionary<int, IList<Task>> _capturedTasks = new Dictionary<int, IList<Task>>();
        private readonly object _lock = new object();

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return Enumerable.Empty<Task>();
        }

        protected override void QueueTask(Task task)
        {
            Add(task);

            Task.Run(() => TryExecuteTask(task));
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (taskWasPreviouslyQueued)
            {
                return false;
            }

            Add(task);

            return TryExecuteTask(task);
        }

        private void Add(Task task)
        {
            lock (_lock)
            {
                IList<Task> tasks = null;
                if (_capturedTasks.TryGetValue(Task.CurrentId ?? 0, out tasks))
                {
                    tasks.Add(task);
                }
                else
                {
                    _capturedTasks[Task.CurrentId ?? 0] = new[] { task };
                }
            }
        }

        public IDictionary<int, IList<Task>> CapturedTasksBySchedulingTask
        {
            get
            {
                lock (_lock)
                {
                    return new Dictionary<int, IList<Task>>(_capturedTasks);
                }
            }
        }

        public IEnumerable<Task> AllCapturedTasks
        {
            get
            {
                return CapturedTasksBySchedulingTask.SelectMany(kv => kv.Value);
            }
        }
    }
}
