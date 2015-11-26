// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class TaskContinueWhenAllTests
    {
        /// <summary>
        /// Get a set of data with different workloads
        /// </summary>
        /// Returned format is:
        ///  1. Array of workloads
        /// <returns>Workloads</returns>
        public static IEnumerable<object[]> Workload_Data()
        {
            foreach (TimeSpan workload in new[] { Workloads.VeryLight, Workloads.Light, Workloads.Medium })
            {
                yield return new object[] { new[] { workload } };
            }

            yield return new object[] { new[] { Workloads.Medium, Workloads.VeryLight, Workloads.Medium, Workloads.Light } };
            yield return new object[] { new object[] { Workloads.Medium, Workloads.Light, Workloads.Heavy, Workloads.VeryLight, Workloads.VeryHeavy, Workloads.Medium } };
        }

        /// <summary>
        /// Get a set of data for testing continuation options with different workloads.
        /// </summary>
        /// Returned format is:
        ///  1. Array of workloads
        ///  2. Continuation options
        /// <returns>Workloads and options</returns>
        public static IEnumerable<object[]> OptionData()
        {
            foreach (TaskContinuationOptions option in new[] {
                    TaskContinuationOptions.AttachedToParent,
                    TaskContinuationOptions.DenyChildAttach,
                    TaskContinuationOptions.HideScheduler,
                    TaskContinuationOptions.LongRunning,
                    TaskContinuationOptions.None,
                    TaskContinuationOptions.PreferFairness, })
            {
                foreach (TimeSpan workload in new[] { Workloads.VeryLight, Workloads.Light, Workloads.Medium })
                {
                    yield return new object[] { new[] { workload }, option };
                }
                yield return new object[] { new[] { Workloads.VeryLight, Workloads.VeryLight, Workloads.Light }, option };
            }
            yield return new object[] { new[] { Workloads.Medium, Workloads.VeryLight, Workloads.Medium, Workloads.Light }, TaskContinuationOptions.None };
            yield return new object[] { new object[] { Workloads.Medium, Workloads.Light, Workloads.Heavy, Workloads.VeryLight, Workloads.VeryHeavy, Workloads.Medium }, TaskContinuationOptions.LongRunning };
        }

        [Theory]
        [MemberData("Workload_Data")]
        public static void Factory_Future_Future(TimeSpan[] workloads)
        {
            Future_Future(workloads, tasks => new TaskFactory().ContinueWhenAll(tasks, completed => AssertAllComplete(completed)));
        }

        [Theory]
        [MemberData("Workload_Data")]
        public static void Factory_Result_Future_Future(TimeSpan[] workloads)
        {
            Future_Future(workloads, tasks => new TaskFactory<int>().ContinueWhenAll(tasks, completed => AssertAllComplete(completed)));
        }

        private static Task<int> Future_Future(TimeSpan[] workloads, Func<Task<int>[], Task<int>> factory)
        {
            Task<int>[] tasks = workloads.Select((load, i) => new Task<int>(() => Functions.SpinAndReturn(load, i + 1))).ToArray();
            Task<int> cont = factory(tasks);

            Assert.All(tasks, task => Assert.Equal(TaskStatus.Created, task.Status));
            Assert.Equal(TaskStatus.WaitingForActivation, cont.Status);

            tasks.ForAll(task => task.Start());
            cont.Wait();
            AssertComplete(tasks.Length, cont);
            return cont;
        }

        [Theory]
        [MemberData("Workload_Data")]
        public static void Factory_Task_Future(TimeSpan[] loads)
        {
            Task_Future(loads, (tasks, data) => new TaskFactory().ContinueWhenAll(tasks, completed => AssertAllComplete(completed, data)));
        }

        [Theory]
        [MemberData("Workload_Data")]
        public static void Factory_Result_Task_Future(TimeSpan[] loads)
        {
            Task_Future(loads, (tasks, data) => new TaskFactory<int>().ContinueWhenAll(tasks, completed => AssertAllComplete(completed, data)));
        }

        private static Task<int> Task_Future(TimeSpan[] loads, Func<Task[], int[], Task<int>> factory)
        {
            int[] data = new int[loads.Length];
            Task[] tasks = loads.Select((load, i) => new Task(() => SpinAndCheck(load, i, data))).ToArray();
            Task<int> cont = factory(tasks, data);

            Assert.All(tasks, task => Assert.Equal(TaskStatus.Created, task.Status));
            Assert.Equal(TaskStatus.WaitingForActivation, cont.Status);

            tasks.ForAll(task => task.Start());
            cont.Wait();
            AssertComplete(tasks.Length, cont);
            return cont;
        }

        [Theory]
        [MemberData("Workload_Data")]
        public static void Factory_Task_Task(TimeSpan[] loads)
        {
            Task_Task(loads, (tasks, data) => new TaskFactory().ContinueWhenAll(tasks, completed =>
            {
                int counted = AssertAllComplete(completed, data);
                Assert.Equal(loads.Length, counted);
            }));
        }

        private static Task Task_Task(TimeSpan[] loads, Func<Task[], int[], Task> factory)
        {
            int[] data = new int[loads.Length];
            Task[] tasks = loads.Select((load, i) => new Task(() => SpinAndCheck(load, i, data))).ToArray();
            Task cont = factory(tasks, data);

            Assert.All(tasks, task => Assert.Equal(TaskStatus.Created, task.Status));
            Assert.Equal(TaskStatus.WaitingForActivation, cont.Status);

            tasks.ForAll(task => task.Start());
            cont.Wait();
            AssertComplete(loads.Length, cont, tasks.Length);
            return cont;
        }

        private static void SpinAndCheck(TimeSpan load, int i, int[] data)
        {
            Functions.SpinAndDo(load, () => Assert.Equal(0, Interlocked.CompareExchange(ref data[i], i + 1, 0)));
        }

        [Theory]
        [MemberData("Workload_Data")]
        public static void Factory_Future_Task(TimeSpan[] loads)
        {
            Future_Task(loads, tasks => new TaskFactory().ContinueWhenAll(tasks, completed =>
            {
                int counted = AssertAllComplete(completed);
                Assert.Equal(loads.Length, completed.Length);
            }));
        }

        private static Task Future_Task(TimeSpan[] loads, Func<Task<int>[], Task> factory)
        {
            Task<int>[] tasks = loads.Select((load, i) => new Task<int>(() => Functions.SpinAndReturn(load, i + 1))).ToArray();

            Task cont = factory(tasks);

            Assert.All(tasks, task => Assert.Equal(TaskStatus.Created, task.Status));
            Assert.Equal(TaskStatus.WaitingForActivation, cont.Status);

            tasks.ForAll(task => task.Start());
            cont.Wait();
            AssertComplete(loads.Length, cont, tasks.Length);
            return cont;
        }

        [Theory]
        [MemberData("Workload_Data")]
        public static void Factory_Future_Future_Token(TimeSpan[] loads)
        {
            Future_Future(loads, tasks => new TaskFactory().ContinueWhenAll(tasks, completed => AssertAllComplete(completed), new CancellationToken()));
        }

        [Theory]
        [MemberData("Workload_Data")]
        public static void Factory_Result_Future_Future_Token(TimeSpan[] loads)
        {
            Future_Future(loads, tasks => new TaskFactory<int>().ContinueWhenAll(tasks, completed => AssertAllComplete(completed), new CancellationToken()));
        }

        [Theory]
        [MemberData("Workload_Data")]
        public static void Factory_Task_Future_Token(TimeSpan[] loads)
        {
            Task_Future(loads, (tasks, data) => new TaskFactory().ContinueWhenAll(tasks, completed => AssertAllComplete(completed, data), new CancellationToken()));
        }

        [Theory]
        [MemberData("Workload_Data")]
        public static void Factory_Result_Task_Future_Token(TimeSpan[] loads)
        {
            Task_Future(loads, (tasks, data) => new TaskFactory<int>().ContinueWhenAll(tasks, completed => AssertAllComplete(completed, data), new CancellationToken()));
        }

        [Theory]
        [MemberData("Workload_Data")]
        public static void Factory_Task_Task_Token(TimeSpan[] loads)
        {
            Task_Task(loads, (tasks, data) => new TaskFactory().ContinueWhenAll(tasks, completed =>
             {
                 int counted = AssertAllComplete(completed, data);
                 Assert.Equal(loads.Length, counted);
             }, new CancellationToken()));
        }

        [Theory]
        [MemberData("Workload_Data")]
        public static void Factory_Future_Task_Token(TimeSpan[] loads)
        {
            Future_Task(loads, tasks => new TaskFactory().ContinueWhenAll(tasks, completed =>
            {
                int counted = AssertAllComplete(completed);
                Assert.Equal(loads.Length, completed.Length);
            }, new CancellationToken()));
        }

        [Theory]
        [MemberData("OptionData")]
        public static void Factory_Future_Future_Options(TimeSpan[] loads, TaskContinuationOptions options)
        {
            Task<int> task = Future_Future(loads, tasks => new TaskFactory().ContinueWhenAll(tasks, completed => AssertAllComplete(completed), options));
            Assert.Equal(options, (TaskContinuationOptions)task.CreationOptions);
        }

        [Theory]
        [MemberData("OptionData")]
        public static void Factory_Result_Future_Future_Options(TimeSpan[] loads, TaskContinuationOptions options)
        {
            Task<int> task = Future_Future(loads, tasks => new TaskFactory<int>().ContinueWhenAll(tasks, completed => AssertAllComplete(completed), options));
            Assert.Equal(options, (TaskContinuationOptions)task.CreationOptions);
        }

        [Theory]
        [MemberData("OptionData")]
        public static void Factory_Task_Future_Options(TimeSpan[] loads, TaskContinuationOptions options)
        {
            Task<int> task = Task_Future(loads, (tasks, data) => new TaskFactory().ContinueWhenAll(tasks, completed => AssertAllComplete(completed, data), options));
            Assert.Equal(options, (TaskContinuationOptions)task.CreationOptions);
        }

        [Theory]
        [MemberData("OptionData")]
        public static void Factory_Result_Task_Future_Options(TimeSpan[] loads, TaskContinuationOptions options)
        {
            Task<int> task = Task_Future(loads, (tasks, data) => new TaskFactory<int>().ContinueWhenAll(tasks, completed => AssertAllComplete(completed, data), options));
            Assert.Equal(options, (TaskContinuationOptions)task.CreationOptions);
        }

        [Theory]
        [MemberData("OptionData")]
        public static void Factory_Task_Task_Options(TimeSpan[] loads, TaskContinuationOptions options)
        {
            Task task = Task_Task(loads, (tasks, data) => new TaskFactory().ContinueWhenAll(tasks, completed =>
            {
                int counted = AssertAllComplete(completed, data);
                Assert.Equal(loads.Length, counted);
            }, options));
            Assert.Equal(options, (TaskContinuationOptions)task.CreationOptions);
        }

        [Theory]
        [MemberData("OptionData")]
        public static void Factory_Future_Task_Options(TimeSpan[] loads, TaskContinuationOptions options)
        {
            Task task = Future_Task(loads, tasks => new TaskFactory().ContinueWhenAll(tasks, completed =>
              {
                  int counted = AssertAllComplete(completed);
                  Assert.Equal(loads.Length, completed.Length);
              }, options));
            Assert.Equal(options, (TaskContinuationOptions)task.CreationOptions);
        }

        [Theory]
        [MemberData("OptionData")]
        public static void Factory_Future_Future_Scheduler(TimeSpan[] loads, TaskContinuationOptions options)
        {
            TaskScheduler scheduler = new QUWITaskScheduler();
            TaskScheduler actual = null;
            Task<int> task = Future_Future(loads, tasks => new TaskFactory().ContinueWhenAll(tasks, completed =>
            {
                actual = TaskScheduler.Current;
                return AssertAllComplete(completed);
            }, new CancellationToken(), options, scheduler));

            if (options == TaskContinuationOptions.HideScheduler)
            {
                Assert.Equal(TaskScheduler.Default, actual);
            }
            else
            {
                Assert.Equal(scheduler, actual);
            }
            Assert.Equal(options, (TaskContinuationOptions)task.CreationOptions);
        }

        [Theory]
        [MemberData("OptionData")]
        public static void Factory_Result_Future_Future_Scheduler(TimeSpan[] loads, TaskContinuationOptions options)
        {
            TaskScheduler scheduler = new QUWITaskScheduler();
            TaskScheduler actual = null;
            Task<int> task = Future_Future(loads, tasks => new TaskFactory<int>().ContinueWhenAll(tasks, completed =>
            {
                actual = TaskScheduler.Current;
                return AssertAllComplete(completed);
            }, new CancellationToken(), options, scheduler));

            if (options == TaskContinuationOptions.HideScheduler)
            {
                Assert.Equal(TaskScheduler.Default, actual);
            }
            else
            {
                Assert.Equal(scheduler, actual);
            }
            Assert.Equal(options, (TaskContinuationOptions)task.CreationOptions);
        }

        [Theory]
        [MemberData("OptionData")]
        public static void Factory_Task_Future_Scheduler(TimeSpan[] loads, TaskContinuationOptions options)
        {
            TaskScheduler scheduler = new QUWITaskScheduler();
            TaskScheduler actual = null;
            Task<int> task = Task_Future(loads, (tasks, data) => new TaskFactory().ContinueWhenAll(tasks, completed =>
            {
                actual = TaskScheduler.Current;
                return AssertAllComplete(completed, data);
            }, new CancellationToken(), options, scheduler));

            if (options == TaskContinuationOptions.HideScheduler)
            {
                Assert.Equal(TaskScheduler.Default, actual);
            }
            else
            {
                Assert.Equal(scheduler, actual);
            }
            Assert.Equal(options, (TaskContinuationOptions)task.CreationOptions);
        }

        [Theory]
        [MemberData("OptionData")]
        public static void Factory_Result_Task_Future_Scheduler(TimeSpan[] loads, TaskContinuationOptions options)
        {
            TaskScheduler scheduler = new QUWITaskScheduler();
            TaskScheduler actual = null;
            Task<int> task = Task_Future(loads, (tasks, data) => new TaskFactory<int>().ContinueWhenAll(tasks, completed =>
            {
                actual = TaskScheduler.Current;
                return AssertAllComplete(completed, data);
            }, new CancellationToken(), options, scheduler));

            if (options == TaskContinuationOptions.HideScheduler)
            {
                Assert.Equal(TaskScheduler.Default, actual);
            }
            else
            {
                Assert.Equal(scheduler, actual);
            }
            Assert.Equal(options, (TaskContinuationOptions)task.CreationOptions);
        }

        [Theory]
        [MemberData("OptionData")]
        public static void Factory_Task_Task_Scheduler(TimeSpan[] loads, TaskContinuationOptions options)
        {
            TaskScheduler scheduler = new QUWITaskScheduler();
            TaskScheduler actual = null;
            Task task = Task_Task(loads, (tasks, data) => new TaskFactory().ContinueWhenAll(tasks, completed =>
            {
                actual = TaskScheduler.Current;
                int counted = AssertAllComplete(completed, data);
                Assert.Equal(loads.Length, counted);
            }, new CancellationToken(), options, scheduler));

            if (options == TaskContinuationOptions.HideScheduler)
            {
                Assert.Equal(TaskScheduler.Default, actual);
            }
            else
            {
                Assert.Equal(scheduler, actual);
            }
            Assert.Equal(options, (TaskContinuationOptions)task.CreationOptions);
        }

        [Theory]
        [MemberData("OptionData")]
        public static void Factory_Future_Task_Scheduler(TimeSpan[] loads, TaskContinuationOptions options)
        {
            TaskScheduler scheduler = new QUWITaskScheduler();
            TaskScheduler actual = null;
            Task task = Future_Task(loads, tasks => new TaskFactory().ContinueWhenAll(tasks, completed =>
            {
                actual = TaskScheduler.Current;
                int counted = AssertAllComplete(completed);
                Assert.Equal(loads.Length, completed.Length);
            }, new CancellationToken(), options, scheduler));

            if (options == TaskContinuationOptions.HideScheduler)
            {
                Assert.Equal(TaskScheduler.Default, actual);
            }
            else
            {
                Assert.Equal(scheduler, actual);
            }
            Assert.Equal(options, (TaskContinuationOptions)task.CreationOptions);
        }

        [Theory]
        [InlineData(TaskContinuationOptions.AttachedToParent)]
        [InlineData(TaskContinuationOptions.DenyChildAttach)]
        [InlineData(TaskContinuationOptions.ExecuteSynchronously)]
        [InlineData(TaskContinuationOptions.HideScheduler)]
        [InlineData(TaskContinuationOptions.LazyCancellation)]
        [InlineData(TaskContinuationOptions.LongRunning)]
        [InlineData(TaskContinuationOptions.None)]
        [InlineData(TaskContinuationOptions.PreferFairness)]
        public static void ContinueWhenAll_CanceledToken(TaskContinuationOptions options)
        {
            AssertCanceledWithSameToken(token => new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, c => 0, token));
            AssertCanceledWithSameToken(token => new TaskFactory<int>().ContinueWhenAll(new[] { Task.CompletedTask }, c => 0, token));
            AssertCanceledWithSameToken(token => new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, c => 0, token));
            AssertCanceledWithSameToken(token => new TaskFactory<int>().ContinueWhenAll(new[] { Task.FromResult(0) }, c => 0, token));
            AssertCanceledWithSameToken(token => new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, c => { }, token));
            AssertCanceledWithSameToken(token => new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, c => { }, token));

            AssertCanceledWithSameToken(token => new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, c => 0, token, options, TaskScheduler.Default));
            AssertCanceledWithSameToken(token => new TaskFactory<int>().ContinueWhenAll(new[] { Task.CompletedTask }, c => 0, token, options, TaskScheduler.Default));
            AssertCanceledWithSameToken(token => new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, c => 0, token, options, TaskScheduler.Default));
            AssertCanceledWithSameToken(token => new TaskFactory<int>().ContinueWhenAll(new[] { Task.FromResult(0) }, c => 0, token, options, TaskScheduler.Default));
            AssertCanceledWithSameToken(token => new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, c => { }, token, options, TaskScheduler.Default));
            AssertCanceledWithSameToken(token => new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, c => { }, token, options, TaskScheduler.Default));
        }

        private static void AssertCanceledWithSameToken(Func<CancellationToken, Task> factory)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            source.Cancel();

            Task cont = factory(token);

            AggregateException ae = Assert.Throws<AggregateException>(() => cont.Wait());
            TaskCanceledException tce = Assert.IsType<TaskCanceledException>(ae.InnerException);
            Assert.Equal(token, tce.CancellationToken);
            Assert.True(cont.IsCanceled);
            Assert.True(cont.IsCompleted);
            Assert.False(cont.IsFaulted);
            Assert.Equal(TaskStatus.Canceled, cont.Status);
        }

        [Theory]
        [InlineData(TaskContinuationOptions.AttachedToParent)]
        [InlineData(TaskContinuationOptions.DenyChildAttach)]
        [InlineData(TaskContinuationOptions.ExecuteSynchronously)]
        [InlineData(TaskContinuationOptions.HideScheduler)]
        [InlineData(TaskContinuationOptions.LazyCancellation)]
        [InlineData(TaskContinuationOptions.LongRunning)]
        [InlineData(TaskContinuationOptions.None)]
        [InlineData(TaskContinuationOptions.PreferFairness)]
        public static void ContinueWhenAll_Canceled_Exception_Tasks(TaskContinuationOptions options)
        {
            AssertSameTask(Task.FromCanceled(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }));
            AssertSameTask(Task.FromCanceled(new CancellationToken(true)),
                (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }));
            AssertSameTask(Task.FromCanceled<int>(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }));
            AssertSameTask(Task.FromCanceled<int>(new CancellationToken(true)),
                (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }));
            AssertSameTask(Task.FromCanceled(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }));
            AssertSameTask(Task.FromCanceled<int>(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }));

            AssertSameTask(Task.FromCanceled(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken()));
            AssertSameTask(Task.FromCanceled(new CancellationToken(true)),
                (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken()));
            AssertSameTask(Task.FromCanceled<int>(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken()));
            AssertSameTask(Task.FromCanceled<int>(new CancellationToken(true)),
               (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken()));
            AssertSameTask(Task.FromCanceled(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }, new CancellationToken()));
            AssertSameTask(Task.FromCanceled<int>(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }, new CancellationToken()));

            AssertSameTask(Task.FromCanceled(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, options));
            AssertSameTask(Task.FromCanceled(new CancellationToken(true)),
                (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, options));
            AssertSameTask(Task.FromCanceled<int>(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, options));
            AssertSameTask(Task.FromCanceled<int>(new CancellationToken(true)),
                (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, options));
            AssertSameTask(Task.FromCanceled(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }, options));
            AssertSameTask(Task.FromCanceled<int>(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }, options));

            AssertSameTask(Task.FromCanceled(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken(), options, TaskScheduler.Default));
            AssertSameTask(Task.FromCanceled(new CancellationToken(true)),
                (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken(), options, TaskScheduler.Default));
            AssertSameTask(Task.FromCanceled<int>(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken(), options, TaskScheduler.Default));
            AssertSameTask(Task.FromCanceled<int>(new CancellationToken(true)),
               (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken(), options, TaskScheduler.Default));
            AssertSameTask(Task.FromCanceled(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }, new CancellationToken(), options, TaskScheduler.Default));
            AssertSameTask(Task.FromCanceled<int>(new CancellationToken(true)),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }, new CancellationToken(), options, TaskScheduler.Default));

            AssertSameTask(Task.FromException(new DeliberateTestException()),
               (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }));
            AssertSameTask(Task.FromException(new DeliberateTestException()),
                (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }));
            AssertSameTask(Task.FromException<int>(new DeliberateTestException()),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }));
            AssertSameTask(Task.FromException<int>(new DeliberateTestException()),
                (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }));
            AssertSameTask(Task.FromException(new DeliberateTestException()),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }));
            AssertSameTask(Task.FromException<int>(new DeliberateTestException()),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }));

            AssertSameTask(Task.FromException(new DeliberateTestException()),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken()));
            AssertSameTask(Task.FromException(new DeliberateTestException()),
                (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken()));
            AssertSameTask(Task.FromException<int>(new DeliberateTestException()),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken()));
            AssertSameTask(Task.FromException<int>(new DeliberateTestException()),
               (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken()));
            AssertSameTask(Task.FromException(new DeliberateTestException()),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }, new CancellationToken()));
            AssertSameTask(Task.FromException<int>(new DeliberateTestException()),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }, new CancellationToken()));

            AssertSameTask(Task.FromException(new DeliberateTestException()),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, options));
            AssertSameTask(Task.FromException(new DeliberateTestException()),
                (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, options));
            AssertSameTask(Task.FromException<int>(new DeliberateTestException()),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, options));
            AssertSameTask(Task.FromException<int>(new DeliberateTestException()),
                (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, options));
            AssertSameTask(Task.FromException(new DeliberateTestException()),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }, options));
            AssertSameTask(Task.FromException<int>(new DeliberateTestException()),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }, options));

            AssertSameTask(Task.FromException(new DeliberateTestException()),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken(), options, TaskScheduler.Default));
            AssertSameTask(Task.FromException(new DeliberateTestException()),
                (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken(), options, TaskScheduler.Default));
            AssertSameTask(Task.FromException<int>(new DeliberateTestException()),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken(), options, TaskScheduler.Default));
            AssertSameTask(Task.FromException<int>(new DeliberateTestException()),
               (task, flag) => new TaskFactory<int>().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); return 0; }, new CancellationToken(), options, TaskScheduler.Default));
            AssertSameTask(Task.FromException(new DeliberateTestException()),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }, new CancellationToken(), options, TaskScheduler.Default));
            AssertSameTask(Task.FromException<int>(new DeliberateTestException()),
                (task, flag) => new TaskFactory().ContinueWhenAll(new[] { task }, c => { Assert.Same(task, c.Single()); flag.Trip(); }, new CancellationToken(), options, TaskScheduler.Default));
        }

        private static void AssertSameTask<T>(T task, Func<T, Flag, Task> factory) where T : Task
        {
            Flag f = new Flag();
            Task cont = factory(task, f);
            cont.Wait();
            Assert.True(f.IsTripped);
            AssertComplete(1, cont, 1);
        }

        [Theory]
        [InlineData(TaskContinuationOptions.NotOnCanceled)]
        [InlineData(TaskContinuationOptions.NotOnFaulted)]
        [InlineData(TaskContinuationOptions.NotOnRanToCompletion)]
        [InlineData(TaskContinuationOptions.OnlyOnCanceled)]
        [InlineData(TaskContinuationOptions.OnlyOnFaulted)]
        [InlineData(TaskContinuationOptions.OnlyOnRanToCompletion)]
        [InlineData(TaskContinuationOptions.RunContinuationsAsynchronously)]
        [InlineData(TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.LongRunning)]
        public static void ContinueWhenAll_Option_ArgumentOutOfRange(TaskContinuationOptions options)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, c => 0, options); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, c => 0, options); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new TaskFactory<int>().ContinueWhenAll(new[] { Task.CompletedTask }, c => 0, options); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, c => 0, options); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new TaskFactory<int>().ContinueWhenAll(new[] { Task.FromResult(0) }, c => 0, options); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, c => { }, options); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, c => { }, options); });

            Assert.Throws<ArgumentOutOfRangeException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, c => 0, new CancellationToken(), options, TaskScheduler.Default); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new TaskFactory<int>().ContinueWhenAll(new[] { Task.CompletedTask }, c => 0, new CancellationToken(), options, TaskScheduler.Default); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, c => 0, new CancellationToken(), options, TaskScheduler.Default); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new TaskFactory<int>().ContinueWhenAll(new[] { Task.FromResult(0) }, c => 0, new CancellationToken(), options, TaskScheduler.Default); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, c => { }, new CancellationToken(), options, TaskScheduler.Default); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, c => { }, new CancellationToken(), options, TaskScheduler.Default); });
        }

        [Fact]
        public static void ContinueWhenAll_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { }, c => 0); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { null }, c => 0); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task[] { }, c => 0); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task[] { null }, c => 0); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { }, c => 0); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { null }, c => 0); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task<int>[] { }, c => 0); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task<int>[] { null }, c => 0); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { }, c => { }); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { null }, c => { }); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { }, c => { }); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { null }, c => { }); });

            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { }, c => 0, new CancellationToken()); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { null }, c => 0, new CancellationToken()); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task[] { }, c => 0, new CancellationToken()); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task[] { null }, c => 0, new CancellationToken()); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { }, c => 0, new CancellationToken()); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { null }, c => 0, new CancellationToken()); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task<int>[] { }, c => 0, new CancellationToken()); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task<int>[] { null }, c => 0, new CancellationToken()); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { }, c => { }, new CancellationToken()); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { null }, c => { }, new CancellationToken()); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { }, c => { }, new CancellationToken()); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { null }, c => { }, new CancellationToken()); });

            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { }, c => 0, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { null }, c => 0, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task[] { }, c => 0, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task[] { null }, c => 0, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { }, c => 0, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { null }, c => 0, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task<int>[] { }, c => 0, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task<int>[] { null }, c => 0, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { }, c => { }, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { null }, c => { }, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { }, c => { }, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { null }, c => { }, TaskContinuationOptions.None); });

            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { }, c => 0, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { null }, c => 0, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task[] { }, c => 0, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task[] { null }, c => 0, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { }, c => 0, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { null }, c => 0, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task<int>[] { }, c => 0, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory<int>().ContinueWhenAll(new Task<int>[] { null }, c => 0, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { }, c => { }, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task[] { null }, c => { }, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { }, c => { }, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentException>(() => { new TaskFactory().ContinueWhenAll(new Task<int>[] { null }, c => { }, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
        }

        [Fact]
        public static void ContinueWhenAll_ArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task[])null, c => 0); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, (Func<Task[], int>)null); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll((Task[])null, c => 0); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll(new[] { Task.CompletedTask }, (Func<Task[], int>)null); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task<int>[])null, c => 0); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, (Func<Task<int>[], int>)null); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll((Task<int>[])null, c => 0); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll(new[] { Task.FromResult(0) }, (Func<Task<int>[], int>)null); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task[])null, c => { }); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, (Action<Task[]>)null); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task<int>[])null, c => { }); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, (Action<Task<int>[]>)null); });

            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task[])null, c => 0, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, (Func<Task[], int>)null, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll((Task[])null, c => 0, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll(new[] { Task.CompletedTask }, (Func<Task[], int>)null, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task<int>[])null, c => 0, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, (Func<Task<int>[], int>)null, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll((Task<int>[])null, c => 0, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll(new[] { Task.FromResult(0) }, (Func<Task<int>[], int>)null, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task[])null, c => { }, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, (Action<Task[]>)null, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task<int>[])null, c => { }, new CancellationToken()); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, (Action<Task<int>[]>)null, new CancellationToken()); });

            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task[])null, c => 0, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, (Func<Task[], int>)null, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll((Task[])null, c => 0, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll(new[] { Task.CompletedTask }, (Func<Task[], int>)null, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task<int>[])null, c => 0, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, (Func<Task<int>[], int>)null, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll((Task<int>[])null, c => 0, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll(new[] { Task.FromResult(0) }, (Func<Task<int>[], int>)null, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task[])null, c => { }, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, (Action<Task[]>)null, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task<int>[])null, c => { }, TaskContinuationOptions.None); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, (Action<Task<int>[]>)null, TaskContinuationOptions.None); });

            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task[])null, c => 0, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, (Func<Task[], int>)null, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, c => 0, new CancellationToken(), TaskContinuationOptions.None, null); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll((Task[])null, c => 0, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll(new[] { Task.CompletedTask }, (Func<Task[], int>)null, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll(new[] { Task.CompletedTask }, c => 0, new CancellationToken(), TaskContinuationOptions.None, null); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task<int>[])null, c => 0, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, (Func<Task<int>[], int>)null, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, c => 0, new CancellationToken(), TaskContinuationOptions.None, null); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll((Task<int>[])null, c => 0, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll(new[] { Task.FromResult(0) }, (Func<Task<int>[], int>)null, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory<int>().ContinueWhenAll(new[] { Task.FromResult(0) }, c => 0, new CancellationToken(), TaskContinuationOptions.None, null); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task[])null, c => { }, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, (Action<Task[]>)null, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.CompletedTask }, c => { }, new CancellationToken(), TaskContinuationOptions.None, null); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll((Task<int>[])null, c => { }, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, (Action<Task<int>[]>)null, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.Default); });
            Assert.Throws<ArgumentNullException>(() => { new TaskFactory().ContinueWhenAll(new[] { Task.FromResult(0) }, c => { }, new CancellationToken(), TaskContinuationOptions.None, null); });
        }

        private static void ForAll<T>(this T[] elements, Action<T> action)
        {
            foreach (T element in elements)
            {
                action(element);
            }
        }

        private static int AssertAllComplete(Task[] completed, int[] data)
        {
            int counter = 1;
            Assert.All(completed, task => AssertComplete(counter, task, data[counter++ - 1]));
            return completed.Length;
        }

        private static void AssertComplete<T>(T expected, Task task, T data)
        {
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.True(task.IsCompleted);
            Assert.False(task.IsFaulted);
            Assert.False(task.IsCanceled);
            Assert.Equal(expected, data);
        }

        private static int AssertAllComplete(Task<int>[] completed)
        {
            int counter = 1;
            Assert.All(completed, task => AssertComplete(counter++, task));
            return completed.Length;
        }

        private static void AssertComplete<T>(T expected, Task<T> task)
        {
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.True(task.IsCompleted);
            Assert.False(task.IsFaulted);
            Assert.False(task.IsCanceled);
            Assert.Equal(expected, task.Result);
        }
    }
}
