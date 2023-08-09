using System.Text;

using RequestSDK.Test.Exceptions;

namespace RequestSDK.Test.Base;

public partial class FixtureBase
{
    protected abstract class CheckHelper
    {
        protected delegate void SyncMethodHandler();
        protected internal Queue<Func<Task<string?>>> CheckTasks = new();
        protected abstract string RevisorName { get; }
        protected void AsParallel(Action revision) => CheckTasks.Enqueue(() =>
        {
            Exception exception = Record.Exception(() => revision.Invoke());
            if(exception != null)
            {
                string? revisionName = revision.Method.ToString();
                int startIndex = revisionName?.IndexOf('<') ?? -1;
                int endIndex = revisionName?.IndexOf('>') ?? -1;

                if (startIndex >= 0 && endIndex >= 0)
                {
                    startIndex++;
                    string revisorMethodName = revisionName!.Substring(startIndex, endIndex - startIndex);
                    return Task.FromResult($"\nRevisor : <{RevisorName}> found exception in method [{revisorMethodName}]. " +
                                           $"\nException message : {exception.Message}")!;
                }
                else
                {
                    return Task.FromResult($"Unhandlered exception was thrown. " +
                                           $"\nRevisor : <{RevisorName}> found exception in anonimous method" +
                                           $"\nException message : {exception.Message}")!;
                }

            }
            return Task.FromResult(default(string));
        });


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
            return exceptionMessageBuilder?.Length > 0 ? new RevisionException(exceptionMessageBuilder.ToString()) : default;
        }
    }
}
