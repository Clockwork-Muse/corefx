// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class Continuation_AsyncState
    {
        [Fact]
        public static void ContinueWith_AsyncState_Null()
        {
            Assert.Null(new Task(() => { }).ContinueWith(_ => { }).AsyncState);
            Assert.Null(new Task(() => { }).ContinueWith(_ => { }, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new Task(() => { }).ContinueWith(_ => { }, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new Task(() => { }).ContinueWith(_ => { }, TaskScheduler.Default).AsyncState);
            Assert.Null(new Task(() => { }).ContinueWith(_ => { }, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Null(new Task(() => { }).ContinueWith(_ => 0).AsyncState);
            Assert.Null(new Task(() => { }).ContinueWith(_ => 0, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new Task(() => { }).ContinueWith(_ => 0, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new Task(() => { }).ContinueWith(_ => 0, TaskScheduler.Default).AsyncState);
            Assert.Null(new Task(() => { }).ContinueWith(_ => 0, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Null(new Task<int>(() => 0).ContinueWith(_ => { }).AsyncState);
            Assert.Null(new Task<int>(() => 0).ContinueWith(_ => { }, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new Task<int>(() => 0).ContinueWith(_ => { }, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new Task<int>(() => 0).ContinueWith(_ => { }, TaskScheduler.Default).AsyncState);
            Assert.Null(new Task<int>(() => 0).ContinueWith(_ => { }, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Null(new Task<int>(() => 0).ContinueWith(_ => 0).AsyncState);
            Assert.Null(new Task<int>(() => 0).ContinueWith(_ => 0, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new Task<int>(() => 0).ContinueWith(_ => 0, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new Task<int>(() => 0).ContinueWith(_ => 0, TaskScheduler.Default).AsyncState);
            Assert.Null(new Task<int>(() => 0).ContinueWith(_ => 0, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);
        }

        [Fact]
        public static void ContinueWhenAll_AsyncState_Null()
        {
            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task(() => { }) }, _ => { }).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task(() => { }) }, _ => { }, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task(() => { }) }, _ => { }, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task(() => { }) }, _ => { }, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task(() => { }) }, _ => 0).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task(() => { }) }, _ => 0, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task(() => { }) }, _ => 0, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task(() => { }) }, _ => 0, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task<int>(() => 0) }, _ => { }).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task<int>(() => 0) }, _ => { }, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task<int>(() => 0) }, _ => { }, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task<int>(() => 0) }, _ => { }, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task<int>(() => 0) }, _ => 0).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task<int>(() => 0) }, _ => 0, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task<int>(() => 0) }, _ => 0, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAll(new[] { new Task<int>(() => 0) }, _ => 0, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Null(new TaskFactory<int>().ContinueWhenAll(new[] { new Task(() => { }) }, _ => 0).AsyncState);
            Assert.Null(new TaskFactory<int>().ContinueWhenAll(new[] { new Task(() => { }) }, _ => 0, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new TaskFactory<int>().ContinueWhenAll(new[] { new Task(() => { }) }, _ => 0, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new TaskFactory<int>().ContinueWhenAll(new[] { new Task(() => { }) }, _ => 0, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Null(new TaskFactory<int>().ContinueWhenAll(new[] { new Task<int>(() => 0) }, _ => 0).AsyncState);
            Assert.Null(new TaskFactory<int>().ContinueWhenAll(new[] { new Task<int>(() => 0) }, _ => 0, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new TaskFactory<int>().ContinueWhenAll(new[] { new Task<int>(() => 0) }, _ => 0, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new TaskFactory<int>().ContinueWhenAll(new[] { new Task<int>(() => 0) }, _ => 0, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);
        }

        [Fact]
        public static void ContinueWhenAny_AsyncState_Null()
        {
            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task(() => { }) }, _ => { }).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task(() => { }) }, _ => { }, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task(() => { }) }, _ => { }, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task(() => { }) }, _ => { }, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task(() => { }) }, _ => 0).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task(() => { }) }, _ => 0, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task(() => { }) }, _ => 0, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task(() => { }) }, _ => 0, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task<int>(() => 0) }, _ => { }).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task<int>(() => 0) }, _ => { }, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task<int>(() => 0) }, _ => { }, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task<int>(() => 0) }, _ => { }, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task<int>(() => 0) }, _ => 0).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task<int>(() => 0) }, _ => 0, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task<int>(() => 0) }, _ => 0, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new TaskFactory().ContinueWhenAny(new[] { new Task<int>(() => 0) }, _ => 0, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Null(new TaskFactory<int>().ContinueWhenAny(new[] { new Task(() => { }) }, _ => 0).AsyncState);
            Assert.Null(new TaskFactory<int>().ContinueWhenAny(new[] { new Task(() => { }) }, _ => 0, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new TaskFactory<int>().ContinueWhenAny(new[] { new Task(() => { }) }, _ => 0, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new TaskFactory<int>().ContinueWhenAny(new[] { new Task(() => { }) }, _ => 0, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Null(new TaskFactory<int>().ContinueWhenAny(new[] { new Task<int>(() => 0) }, _ => 0).AsyncState);
            Assert.Null(new TaskFactory<int>().ContinueWhenAny(new[] { new Task<int>(() => 0) }, _ => 0, new CancellationTokenSource().Token).AsyncState);
            Assert.Null(new TaskFactory<int>().ContinueWhenAny(new[] { new Task<int>(() => 0) }, _ => 0, TaskContinuationOptions.None).AsyncState);
            Assert.Null(new TaskFactory<int>().ContinueWhenAny(new[] { new Task<int>(() => 0) }, _ => 0, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);
        }

        [Fact]
        public static void ContinueWith_AsyncState_NotNull()
        {
            object provided = new object();
            Assert.Equal(provided, new Task(() => { }).ContinueWith((t, o) => { }, provided).AsyncState);
            Assert.Equal(provided, new Task(() => { }).ContinueWith((t, o) => { }, provided, new CancellationTokenSource().Token).AsyncState);
            Assert.Equal(provided, new Task(() => { }).ContinueWith((t, o) => { }, provided, TaskContinuationOptions.None).AsyncState);
            Assert.Equal(provided, new Task(() => { }).ContinueWith((t, o) => { }, provided, TaskScheduler.Default).AsyncState);
            Assert.Equal(provided, new Task(() => { }).ContinueWith((t, o) => { }, provided, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Equal(provided, new Task(() => { }).ContinueWith((t, o) => 0, provided).AsyncState);
            Assert.Equal(provided, new Task(() => { }).ContinueWith((t, o) => 0, provided, new CancellationTokenSource().Token).AsyncState);
            Assert.Equal(provided, new Task(() => { }).ContinueWith((t, o) => 0, provided, TaskContinuationOptions.None).AsyncState);
            Assert.Equal(provided, new Task(() => { }).ContinueWith((t, o) => 0, provided, TaskScheduler.Default).AsyncState);
            Assert.Equal(provided, new Task(() => { }).ContinueWith((t, o) => 0, provided, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Equal(provided, new Task<int>(() => 0).ContinueWith((t, o) => { }, provided).AsyncState);
            Assert.Equal(provided, new Task<int>(() => 0).ContinueWith((t, o) => { }, provided, new CancellationTokenSource().Token).AsyncState);
            Assert.Equal(provided, new Task<int>(() => 0).ContinueWith((t, o) => { }, provided, TaskContinuationOptions.None).AsyncState);
            Assert.Equal(provided, new Task<int>(() => 0).ContinueWith((t, o) => { }, provided, TaskScheduler.Default).AsyncState);
            Assert.Equal(provided, new Task<int>(() => 0).ContinueWith((t, o) => { }, provided, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);

            Assert.Equal(provided, new Task<int>(() => 0).ContinueWith((t, o) => 0, provided).AsyncState);
            Assert.Equal(provided, new Task<int>(() => 0).ContinueWith((t, o) => 0, provided, new CancellationTokenSource().Token).AsyncState);
            Assert.Equal(provided, new Task<int>(() => 0).ContinueWith((t, o) => 0, provided, TaskContinuationOptions.None).AsyncState);
            Assert.Equal(provided, new Task<int>(() => 0).ContinueWith((t, o) => 0, provided, TaskScheduler.Default).AsyncState);
            Assert.Equal(provided, new Task<int>(() => 0).ContinueWith((t, o) => 0, provided, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.Default).AsyncState);
        }
    }
}
