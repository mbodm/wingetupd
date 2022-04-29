using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace WinGet
{
    public sealed class WinGetRunner : IWinGetRunner
    {
        private const string WinGetApp = "winget.exe";
        private const double WinGetAppTimeoutInSeconds = 15;

        public bool WinGetIsInstalled
        {
            get
            {
                try
                {
                    using var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = WinGetApp,
                        Arguments = "--version",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                    });

                    if (process != null && !process.HasExited)
                    {
                        process.Kill();
                    }

                    return true;
                }
                catch (Win32Exception)
                {
                    return false;
                }
            }
        }

        public Task<WinGetRunnerResult> RunWinGetAsync(string parameters, CancellationToken cancellationToken = default) =>
            RunWinGetAsync(parameters, TimeSpan.FromSeconds(WinGetAppTimeoutInSeconds), cancellationToken);

        public async Task<WinGetRunnerResult> RunWinGetAsync(string parameters, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            // Any redirection of StandardError is explicitly omitted here, cause it can lead to various crazy problems
            // when combined with StandardOutput redirection. Also WinGet actually makes no use of it anyway. And if MS
            // ever starts changing such basic WinGet behaviours, we have to adjust many things accordingly here anyway.

            using var process = new Process();

            process.StartInfo = new ProcessStartInfo
            {
                FileName = WinGetApp,
                Arguments = parameters.Trim(),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            var processStarted = false;

            try
            {
                processStarted = process.Start();
            }
            catch (Win32Exception)
            {
                throw new WinGetRunnerException($"{WinGetApp} not installed.");
            }

            if (!processStarted)
            {
                throw new WinGetRunnerException($"{WinGetApp} process not started.");
            }

            // Some typical timeout cancellation pattern here, using CreateLinkedTokenSource() method.

            using var ctsTimeout = new CancellationTokenSource(timeout);
            using var ctsLinked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, ctsTimeout.Token);

            var consoleOutput = string.Empty;

            try
            {
                // Since ReadToEndAsync() has no CancellationToken support before .NET 7,
                // the .WaitAsync() cancellation pattern helps, when timeout has reached.
                // Have a look at the following pages, to get some more infos about this:
                // https://github.com/dotnet/runtime/issues/20824
                // https://andrewlock.net/cancelling-await-calls-in-dotnet-6-with-task-waitasync

                consoleOutput = await process.StandardOutput.ReadToEndAsync().WaitAsync(ctsLinked.Token).ConfigureAwait(false);

                // Since .NET 6 release WaitForExitAsync() waits for redirected Output/Error.
                // Have a look at the comments of the GitHub issue, linked in this blog post:
                // https://www.meziantou.net/process-waitforexitasync-doesn-t-behave-like-process-waitforexit.htm

                await process.WaitForExitAsync(ctsLinked.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ctsTimeout.IsCancellationRequested)
            {
                throw new WinGetRunnerException($"{WinGetApp} reached timeout after {timeout.Seconds} second(s). {WinGetApp} process canceled.");
            }

            var processCall = $"{process.StartInfo.FileName} {process.StartInfo.Arguments}".Trim();

            return new WinGetRunnerResult(processCall, consoleOutput, process.ExitCode);
        }
    }
}
