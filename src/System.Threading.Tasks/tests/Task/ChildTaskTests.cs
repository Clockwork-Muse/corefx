// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class ChildTaskTests
    {
        public static readonly TimeSpan MaxSafeWait = TimeSpan.FromMinutes(1);

        [Fact]
        public static void DenyChildAttach_Factory_StartNew()
        {
            DenyChildAttach(action => new TaskFactory().StartNew(action, TaskCreationOptions.DenyChildAttach));
        }

        [Fact]
        public static void DenyChildAttach_Factory_Int_StartNew()
        {
            DenyChildAttach(action => new TaskFactory<int>().StartNew(() => { action(); return 0; }, TaskCreationOptions.DenyChildAttach));
        }

        [Fact]
        public static void DenyChildAttach_Task_Start()
        {
            DenyChildAttach(action => Start(new Task(action, TaskCreationOptions.DenyChildAttach)));
        }

        [Fact]
        public static void DenyChildAttach_Task_Int_Start()
        {
            DenyChildAttach(action => Start(new Task<int>(() => { action(); return 0; }, TaskCreationOptions.DenyChildAttach)));
        }

        [Fact]
        public static void DenyChildAttach_Continuation()
        {
            DenyChildAttach(action => Task.Delay(TimeSpan.FromMilliseconds(5)).ContinueWith(ignore => { action(); }, TaskContinuationOptions.DenyChildAttach));
        }

        [Fact]
        public static void DenyChildAttach_Continuation_Int()
        {
            DenyChildAttach(action => Task.Delay(TimeSpan.FromMilliseconds(5)).ContinueWith(ignore => { action(); return 0; }, TaskContinuationOptions.DenyChildAttach));
        }

        private static void DenyChildAttach(Func<Action, Task> create)
        {
            using (ManualResetEventSlim mres = new ManualResetEventSlim(false))
            using (Barrier startingLine = new Barrier(3))
            {
                Task child = null;
                // Delegate creation of the parent task to allow easier testing of multiple creation methods.
                Task parent = create(() =>
                {
                    child = new Task(() => { startingLine.SignalAndWait(); mres.Wait(); }, TaskCreationOptions.AttachedToParent);
                    child.Start();
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
