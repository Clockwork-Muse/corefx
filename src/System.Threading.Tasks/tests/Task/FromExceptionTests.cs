// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace System.Threading.Tasks.Tests
{
    public static class FromExceptionTests
    {
        // Internal field references to verify internal exceptions are actually handled.
        private static readonly FieldInfo ContingentPropertiesField = typeof(Task).GetField("m_contingentProperties", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo ExceptionsHolderField = ContingentPropertiesField.FieldType.GetField("m_exceptionsHolder", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo IsHandledField = ExceptionsHolderField.FieldType.GetField("m_isHandled", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public static IEnumerable<object[]> Exception_Data()
        {
            yield return new object[] { new InvalidOperationException() };
            yield return new object[] { new OperationCanceledException() };
            yield return new object[] { new TaskCanceledException() };
            yield return new object[] { new Exception() };
            yield return new object[] { new DeliberateTestException() };
        }

        [Theory]
        [MemberData("Exception_Data")]
        public static void FromException_Task<T>(T exception) where T : Exception
        {
            Task task = Task.FromException(exception);
            Validate(task, exception);

            Assert.NotEqual(task, Task.FromException<int>(exception));
            Assert.NotEqual(task, Task.FromException(exception));
        }

        [Theory]
        [MemberData("Exception_Data")]
        public static void FromException_Future<T>(T exception) where T : Exception
        {
            Task<int> task = Task.FromException<int>(exception);
            Validate(task, exception);

            // retrieving result throws immediately (Completed task)
            Functions.AssertThrowsWrapped<T>(() => { int r = task.Result; });

            Assert.NotEqual(task, Task.FromException<int>(exception));
            Assert.NotEqual(task, Task.FromException(exception));
        }

        private static void Validate<T>(Task task, T exception) where T : Exception
        {
            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.True(task.IsFaulted);
            Assert.NotNull(task.Exception);
            Assert.Equal(exception, task.Exception.InnerException);
            Assert.Equal(TaskStatus.Faulted, task.Status);

            // waiting throws immediately (Completed task)
            Functions.AssertThrowsWrapped<T>(() => task.Wait());

            Assert.Null(task.AsyncState);
            Assert.Equal(TaskCreationOptions.None, task.CreationOptions);
            Assert.Equal(task, task);
        }

        [Theory]
        [MemberData("Exception_Data")]
        public static void FromException_Handled_Task<T>(T exception) where T : Exception
        {
            Task task = Task.FromException(exception);
            Validate_Handled(task, exception);
        }

        [Theory]
        [MemberData("Exception_Data")]
        public static void FromException_Handled_Future<T>(T exception) where T : Exception
        {
            Task<int> task = Task.FromException<int>(exception);
            Validate_Handled(task, exception);
        }

        private static void Validate_Handled<T>(Task task, T exception) where T : Exception
        {
            // Make sure faulted tasks are actually faulted.  There is little choice for this test but to use reflection,
            // as the harness will crash by throwing from the unobserved event if a task goes unhandled with certain runtime flags
            // (everywhere other than here it's a bad thing for an exception to go unobserved)
            object contingentProperties = ContingentPropertiesField.GetValue(task);
            Assert.NotNull(contingentProperties);
            object exceptionsHolder = ExceptionsHolderField.GetValue(contingentProperties);
            Assert.NotNull(exceptionsHolder);

            Assert.False((bool)IsHandledField.GetValue(exceptionsHolder), "Expected FromException task to be unobserved before accessing Exception");
            var ignored = task.Exception;
            Assert.True((bool)IsHandledField.GetValue(exceptionsHolder), "Expected FromException task to be observed after accessing Exception");
        }

        [Fact]
        public static void FromException_NullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => { Task.FromException(null); });
            Assert.Throws<ArgumentNullException>(() => { Task.FromException<int>(null); });
        }
    }
}
