// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class ChildTaskTests
    {
        public static readonly TimeSpan MaxSafeWait = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets child task create funcs
        /// </summary>
        /// Returned data is in format:
        ///  1. string label (for finding failure cases)
        ///  2. Func that takes a Barrier and ManualResetEventSlim, and spits out a started "attached" child task
        /// <returns>A row of data.</returns>
        public static IEnumerable<object[]> ChildTaskCreate_Data()
        {
            yield return new object[] { "Task|Start",
                (Func<Barrier, ManualResetEventSlim, Task>)((b, mres) => Start(new Task(() => { b.SignalAndWait(); mres.Wait(); }, TaskCreationOptions.AttachedToParent))) };
            yield return new object[] { "Task<int>|Start",
                (Func<Barrier, ManualResetEventSlim, Task>)((b, mres) => Start(new Task<int>(() => { b.SignalAndWait(); mres.Wait(); return 0; }, TaskCreationOptions.AttachedToParent))) };
            yield return new object[] { "TaskFactory|StartNew",
                (Func<Barrier, ManualResetEventSlim, Task>)((b, mres) => new TaskFactory().StartNew(() => { b.SignalAndWait(); mres.Wait(); }, TaskCreationOptions.AttachedToParent)) };
            yield return new object[] { "TaskFactory|StartNew<int>",
                (Func<Barrier, ManualResetEventSlim, Task>)((b, mres) => new TaskFactory().StartNew(() => { b.SignalAndWait(); mres.Wait(); return 0;}, TaskCreationOptions.AttachedToParent)) };
            yield return new object[] { "TaskFactory<int>|StartNew",
                (Func<Barrier, ManualResetEventSlim, Task>)((b, mres) => new TaskFactory<int>().StartNew(() => { b.SignalAndWait(); mres.Wait(); return 0; }, TaskCreationOptions.AttachedToParent)) };
            yield return new object[] { "ContinueWith",
                (Func<Barrier, ManualResetEventSlim, Task>)((b, mres) => Task.Delay(1).ContinueWith(i => { b.SignalAndWait(); mres.Wait(); }, TaskContinuationOptions.AttachedToParent)) };
            yield return new object[] { "ContinueWith<int>",
                (Func<Barrier, ManualResetEventSlim, Task>)((b, mres) => Task.Delay(1).ContinueWith(i => { b.SignalAndWait(); mres.Wait(); return 0; }, TaskContinuationOptions.AttachedToParent)) };
        }

        [Theory]
        [MemberData("ChildTaskCreate_Data")]
        public static void DenyChildAttach_Factory_StartNew(string label, Func<Barrier, ManualResetEventSlim, Task> child)
        {
            DenyChildAttach(action => new TaskFactory().StartNew(action, TaskCreationOptions.DenyChildAttach), child);
        }

        [Theory]
        [MemberData("ChildTaskCreate_Data")]
        public static void DenyChildAttach_Factory_Int_StartNew(string label, Func<Barrier, ManualResetEventSlim, Task> child)
        {
            DenyChildAttach(action => new TaskFactory<int>().StartNew(() => { action(); return 0; }, TaskCreationOptions.DenyChildAttach), child);
        }

        [Theory]
        [MemberData("ChildTaskCreate_Data")]
        public static void DenyChildAttach_Task_Start(string label, Func<Barrier, ManualResetEventSlim, Task> child)
        {
            DenyChildAttach(action => Start(new Task(action, TaskCreationOptions.DenyChildAttach)), child);
        }

        [Theory]
        [MemberData("ChildTaskCreate_Data")]
        public static void DenyChildAttach_Task_Int_Start(string label, Func<Barrier, ManualResetEventSlim, Task> child)
        {
            DenyChildAttach(action => Start(new Task<int>(() => { action(); return 0; }, TaskCreationOptions.DenyChildAttach)), child);
        }

        [Theory]
        [MemberData("ChildTaskCreate_Data")]
        public static void DenyChildAttach_Task_Run(string label, Func<Barrier, ManualResetEventSlim, Task> child)
        {
            DenyChildAttach(action => Task.Run(action), child);
        }

        [Theory]
        [MemberData("ChildTaskCreate_Data")]
        public static void DenyChildAttach_Task_Int_Run(string label, Func<Barrier, ManualResetEventSlim, Task> child)
        {
            DenyChildAttach(action => Task.Run(() => { action(); return 0; }), child);
        }

        [Theory]
        [MemberData("ChildTaskCreate_Data")]
        public static void DenyChildAttach_Continuation(string label, Func<Barrier, ManualResetEventSlim, Task> child)
        {
            DenyChildAttach(action => Task.Delay(TimeSpan.FromMilliseconds(5)).ContinueWith(ignore => { action(); }, TaskContinuationOptions.DenyChildAttach), child);
        }

        [Theory]
        [MemberData("ChildTaskCreate_Data")]
        public static void DenyChildAttach_Continuation_Int(string label, Func<Barrier, ManualResetEventSlim, Task> child)
        {
            DenyChildAttach(action => Task.Delay(TimeSpan.FromMilliseconds(5)).ContinueWith(ignore => { action(); return 0; }, TaskContinuationOptions.DenyChildAttach), child);
        }

        private static void DenyChildAttach(Func<Action, Task> createParent, Func<Barrier, ManualResetEventSlim, Task> createChild)
        {
            using (ManualResetEventSlim mres = new ManualResetEventSlim(false))
            using (Barrier startingLine = new Barrier(3))
            {
                Task child = null;
                // Delegate creation of the parent task to allow easier testing of multiple creation methods.
                Task parent = createParent(() =>
                {
                    child = createChild(startingLine, mres);
                    startingLine.SignalAndWait();
                });

                Assert.True(startingLine.SignalAndWait(MaxSafeWait));

                Assert.True(SpinWait.SpinUntil(() => parent.Status != TaskStatus.Running, MaxSafeWait));

                Assert.True(parent.IsCompleted);
                Assert.False(parent.IsCanceled);
                Assert.False(parent.IsFaulted);
                Assert.Null(parent.Exception);
                Assert.Equal(TaskStatus.RanToCompletion, parent.Status);

                Assert.False(child.IsCompleted);
                Assert.Equal(TaskStatus.Running, child.Status);

                mres.Set();
            }
        }

        private static T Start<T>(T task) where T : Task
        {
            task.Start();
            return task;
        }
    }
}
