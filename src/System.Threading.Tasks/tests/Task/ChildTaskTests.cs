// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class ChildTaskTests
    {
        [Fact]
        public static void RunDenyChildAttachTests()
        {
            // StartNew, Task and Future
            Task i1 = null;
            Task t1 = Task.Factory.StartNew(() =>
            {
                i1 = new Task(() => { }, TaskCreationOptions.AttachedToParent);
            }, TaskCreationOptions.DenyChildAttach);

            Task i2 = null;
            Task t2 = Task<int>.Factory.StartNew(() =>
            {
                i2 = new Task(() => { }, TaskCreationOptions.AttachedToParent);
                return 42;
            }, TaskCreationOptions.DenyChildAttach);

            // ctor/Start, Task and Future
            Task i3 = null;
            Task t3 = new Task(() =>
            {
                i3 = new Task(() => { }, TaskCreationOptions.AttachedToParent);
            }, TaskCreationOptions.DenyChildAttach);
            t3.Start();

            Task i4 = null;
            Task t4 = new Task<int>(() =>
            {
                i4 = new Task(() => { }, TaskCreationOptions.AttachedToParent);
                return 42;
            }, TaskCreationOptions.DenyChildAttach);
            t4.Start();

            // continuations, Task and Future
            Task i5 = null;
            Task t5 = t3.ContinueWith(_ =>
            {
                i5 = new Task(() => { }, TaskCreationOptions.AttachedToParent);
            }, TaskContinuationOptions.DenyChildAttach);

            Task i6 = null;
            Task t6 = t4.ContinueWith<int>(_ =>
            {
                i6 = new Task(() => { }, TaskCreationOptions.AttachedToParent);
                return 42;
            }, TaskContinuationOptions.DenyChildAttach);

            // If DenyChildAttach doesn't work in any of the cases, then the associated "parent"
            // task will hang waiting for its child.
            Debug.WriteLine("RunDenyChildAttachTests: Waiting on 'parents' ... if we hang, something went wrong.");
            Task.WaitAll(t1, t2, t3, t4, t5, t6);

            // And clean up.
            i1.Start(); i1.Wait();
            i2.Start(); i2.Wait();
            i3.Start(); i3.Wait();
            i4.Start(); i4.Wait();
            i5.Start(); i5.Wait();
            i6.Start(); i6.Wait();
        }

        private static T Start<T>(T task) where T : Task
        {
            task.Start();
            return task;
        }
    }
}
