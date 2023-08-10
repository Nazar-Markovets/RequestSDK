using System;
using System.Net.Http.Json;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RequestSDK.Test.Base;

public partial class FixtureBase
{
    protected abstract class CheckHelper
    {
        protected delegate void SyncMethodHandler();
        protected internal Queue<Func<Task<string?>>> CheckTasks = new();
        protected abstract string RevisorName { get; }

        /// <summary>Add revision only for <see langword="sync"/> <see cref="Action"/> to <see cref="Queue{T}"/>. That will be executed as parallel task</summary>
        /// <param name="revision">Sync Assert action</param>
        /// <![CDATA[ Example : AsParallelSync(() => Assert.Equal("Expected", "Actual")) ]]>
        protected void AsParallelSync(Action revision) => CheckTasks.Enqueue(() =>
        {
            var isAsyncLambda = revision.Method.IsDefined(typeof(AsyncStateMachineAttribute), false);
            if(isAsyncLambda)
            {
                if(TryClearDelegateMethod(revision, out string? methodName))
                {
                    NotSupportedException notSupportedException = new ($"{Environment.NewLine}Current method [{methodName}] can't pass async lambda in [{nameof(AsParallelSync)}]." + 
                                                                       $"{Environment.NewLine}Use [{nameof(AsParallelAsync)}] instead");
                    return HandleDelegateException(notSupportedException, revision);
                }
            }
                
            Exception exception = Record.Exception(() => revision.Invoke());
            return HandleDelegateException(exception, revision);

        });

        /// <summary>Add revision <see langword="async"/> <see langword="void"/> to <see cref="Queue{T}"/>. That will be executed as parallel task</summary>
        /// <param name="revision"><see langword="async"/> Async Assert action</param>
        /// <![CDATA[ Example : AsParallelAsync(async() => await RevisionOperation()) ]]>
        protected void AsParallelAsync(Func<Task> revision) => CheckTasks.Enqueue(() =>
        {
            Exception exception = Task.Run(async () => await Record.ExceptionAsync(() => revision.Invoke())).Result;
            return HandleDelegateException(exception, revision);
        });


        protected CheckHelper CheckHttpContent<T>(T expectedContent, HttpContent? httpContent)
        {
            if (expectedContent is null) Assert.Fail("Expected content is null");
            if (httpContent is null) Assert.Fail("Actual content is null");
            Type targetGenericType = expectedContent.GetType();
            Type sourceType = typeof(HttpContentJsonExtensions);
            string targetMethodName = nameof(HttpContentJsonExtensions.ReadFromJsonAsync);
            MethodInfo targetMethod = sourceType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                  .Where(method => method.Name == targetMethodName)
                                                  .Where(method => method.IsGenericMethod)
                                                  .Where(method => method.GetGenericArguments().Length == 1)
                                                  .Where(method => method.GetParameters().Any(p => p.ParameterType == typeof(JsonSerializerOptions)))
                                                  .First()
                                                  .MakeGenericMethod(targetGenericType);

            AsParallelAsync(async () =>
            {
                Task serializeTask = (Task)targetMethod.Invoke(null, new object?[] { httpContent, default(JsonSerializerOptions), default(CancellationToken) })!;
                await serializeTask;
                object? actualContent = serializeTask.GetType().GetProperty("Result")?.GetValue(serializeTask);
                Assert.Equal(expectedContent, actualContent);
            });
            return this;
        }

        internal async Task<Exception?> RunParallelChecks()
        {
            var checks = CheckTasks.Select(check => check.Invoke()).ToArray();
            await Task.WhenAll(checks);
            CheckTasks.Clear();
            StringBuilder? exceptionMessageBuilder = null;
            for (int i = 0; i < checks.Length; i++)
            {
                string? exceptionMessage = await checks[i];
                if(exceptionMessage != null)
                {
                    exceptionMessageBuilder ??= new();
                    exceptionMessageBuilder.AppendLine(exceptionMessage);
                }
            }
            return exceptionMessageBuilder?.Length > 0 ? new Exceptions.NotSupportedException(exceptionMessageBuilder.ToString()) : default;
        }

        private static bool TryClearDelegateMethod(Delegate revisionHandler, out string? methodName)
        {
            string? revisionName = revisionHandler.Method.ToString();
            int startIndex = revisionName?.IndexOf('<') ?? -1;
            int endIndex = revisionName?.IndexOf('>') ?? -1;

            if (startIndex >= 0 && endIndex >= 0)
            {
                startIndex++;
                methodName = revisionName![startIndex..endIndex];
            }
            else
            {
                methodName = null;
            }
            return methodName != null;
        }

        private Task<string?> HandleDelegateException(Exception exception, Delegate revisionHandler)
        {
            if (exception != null)
            {
                if(TryClearDelegateMethod(revisionHandler, out string? methodName))
                {
                    return Task.FromResult($"{Environment.NewLine}Revisor : <{RevisorName}> found exception in method [{methodName}]. " +
                                           $"{Environment.NewLine}Exception message : {exception.Message}")!;
                }
                else
                {
                    return Task.FromResult($"Unhandlered exception was thrown. " +
                                           $"{Environment.NewLine}Revisor : <{RevisorName}> found exception in anonimous method" +
                                           $"{Environment.NewLine}Exception message : {exception.Message}")!;
                }
            }
            return Task.FromResult(default(string));
        }
    }
}
