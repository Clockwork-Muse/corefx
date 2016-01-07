// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class DelayTests
    {
        private static readonly TimeSpan MaxSafeWait = TimeSpan.FromMinutes(1);

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public static void Delay_Int_Complete(int milliseconds)
        {
            Delay_Complete(Task.Delay(milliseconds));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public static void Delay_Timespan_Complete(int milliseconds)
        {
            Delay_Complete(Task.Delay(TimeSpan.FromMilliseconds(milliseconds)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public static void Delay_Int_Token_Complete(int milliseconds)
        {
            Delay_Complete(Task.Delay(milliseconds, new CancellationTokenSource().Token));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public static void Delay_Timespan_Token_Complete(int milliseconds)
        {
            Delay_Complete(Task.Delay(TimeSpan.FromMilliseconds(milliseconds), new CancellationTokenSource().Token));
        }

        private static void Delay_Complete(Task delayed)
        {
            Assert.True(SpinWait.SpinUntil(() => delayed.IsCompleted, MaxSafeWait));

            Assert.False(delayed.IsCanceled);
            Assert.False(delayed.IsFaulted);
            Assert.Null(delayed.Exception);
            Assert.Equal(TaskStatus.RanToCompletion, delayed.Status);
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        [InlineData(-1)]
        public static void Delay_Int_NotComplete(int milliseconds)
        {
            Delay_NotComplete(Task.Delay(milliseconds));
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        [InlineData(-1)]
        public static void Delay_Timespan_NotComplete(int milliseconds)
        {
            Delay_NotComplete(Task.Delay(TimeSpan.FromMilliseconds(milliseconds)));
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        [InlineData(-1)]
        public static void Delay_Int_Token_NotComplete(int milliseconds)
        {
            Delay_NotComplete(Task.Delay(milliseconds, new CancellationTokenSource().Token));
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        [InlineData(-1)]
        public static void Delay_Timespan_Token_NotComplete(int milliseconds)
        {
            Delay_NotComplete(Task.Delay(TimeSpan.FromMilliseconds(milliseconds), new CancellationTokenSource().Token));
        }

        private static void Delay_NotComplete(Task delayed)
        {
            SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(5));

            Assert.False(delayed.IsCompleted);
            Assert.False(delayed.IsCanceled);
            Assert.False(delayed.IsFaulted);
            Assert.Null(delayed.Exception);
            Assert.Equal(TaskStatus.WaitingForActivation, delayed.Status);
        }

        [Theory]
        [InlineData(-2)]
        [InlineData(int.MinValue)]
        public static void Delay_OutOfRange(int milliseconds)
        {
            // putting in brackets to prevent use of ThrowsAsync:
            // the exception happens during the call to Delay, not in the returned task
            Assert.Throws<ArgumentOutOfRangeException>(() => { Task.Delay(milliseconds); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { Task.Delay(TimeSpan.FromMilliseconds(milliseconds)); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { Task.Delay(milliseconds, new CancellationTokenSource().Token); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { Task.Delay(TimeSpan.FromMilliseconds(milliseconds), new CancellationTokenSource().Token); });
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        [InlineData(-1)]
        public static void Delay_Int_Token_PreCanceled(int milliseconds)
        {
            Delay_PreCanceled(token => Task.Delay(milliseconds, token));
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        [InlineData(-1)]
        public static void Delay_Timespan_Token_PreCanceled(int milliseconds)
        {
            Delay_PreCanceled(token => Task.Delay(TimeSpan.FromMilliseconds(milliseconds), token));
        }

        private static void Delay_PreCanceled(Func<CancellationToken, Task> create)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.Cancel();

            Task delayed = create(source.Token);

            // Should be canceled immediately
            Assert.True(delayed.IsCompleted);
            Assert.True(delayed.IsCanceled);
            Assert.False(delayed.IsFaulted);
            Assert.Null(delayed.Exception);
            Assert.Equal(TaskStatus.Canceled, delayed.Status);

            AggregateException ae = Assert.Throws<AggregateException>(() => delayed.Wait());
            TaskCanceledException tce = Assert.IsType<TaskCanceledException>(ae.InnerException);
            Assert.Equal(source.Token, tce.CancellationToken);
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        [InlineData(-1)]
        public static void Delay_Int_Token_Cancel(int milliseconds)
        {
            Delay_Cancel(token => Task.Delay(milliseconds, token));
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        [InlineData(-1)]
        public static void Delay_Timespan_Token_Cancel(int milliseconds)
        {
            Delay_Cancel(token => Task.Delay(TimeSpan.FromMilliseconds(milliseconds), token));
        }

        private static void Delay_Cancel(Func<CancellationToken, Task> create)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            Task delayed = create(source.Token);

            // Should be waiting
            Assert.False(delayed.IsCompleted);
            Assert.False(delayed.IsCanceled);
            Assert.False(delayed.IsFaulted);
            Assert.Null(delayed.Exception);
            Assert.Equal(TaskStatus.WaitingForActivation, delayed.Status);

            SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(5));
            source.Cancel();

            // Should be canceled immediately
            Assert.True(delayed.IsCompleted);
            Assert.True(delayed.IsCanceled);
            Assert.False(delayed.IsFaulted);
            Assert.Null(delayed.Exception);
            Assert.Equal(TaskStatus.Canceled, delayed.Status);

            AggregateException ae = Assert.Throws<AggregateException>(() => delayed.Wait());
            TaskCanceledException tce = Assert.IsType<TaskCanceledException>(ae.InnerException);
            Assert.Equal(source.Token, tce.CancellationToken);
        }

        [Fact]
        public static void Delay_Start()
        {
            // Delay tasks are considered "promises", and thus can't be started.
            Assert.Throws<InvalidOperationException>(() => Task.Delay(-1).Start());
            Assert.Throws<InvalidOperationException>(() => Task.Delay(TimeSpan.FromMilliseconds(-1)).Start());
            Assert.Throws<InvalidOperationException>(() => Task.Delay(-1, new CancellationTokenSource().Token).Start());
            Assert.Throws<InvalidOperationException>(() => Task.Delay(TimeSpan.FromMilliseconds(-1), new CancellationTokenSource().Token).Start());
        }
    }
}
