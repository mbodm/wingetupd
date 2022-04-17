using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace WinGet
{
    public sealed class WinGetRunner : IWinGetRunner
    {
        private const string WinGetApp = "winget.exe";
        private const double WinGetAppTimeoutInSeconds = 30;

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

                    if (process != null)
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                    }

                    return true;
                }
                catch (Win32Exception)
                {
                    return false;
                }
            }
        }

        public async Task<WinGetRunnerResult> RunWinGetAsync(string parameters, CancellationToken cancellationToken = default)
        {
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

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

            // Since .NET 6 Process.WaitForExitAsync() waits for redirected Output/Error.
            // Have a look at the comments of the GitHub issue, linked in this blog post:
            // https://www.meziantou.net/process-waitforexitasync-doesn-t-behave-like-process-waitforexit.htm

            var consoleOutput = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
            process.StandardOutput.Close();

            using var ctsTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(WinGetAppTimeoutInSeconds));
            using var ctsLinked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, ctsTimeout.Token);

            try
            {
                await process.WaitForExitAsync(ctsLinked.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ctsTimeout.IsCancellationRequested)
            {
                throw new WinGetRunnerException($"{WinGetApp} reached timeout after {WinGetAppTimeoutInSeconds} seconds. {WinGetApp} process canceled.");
            }

            var processCall = $"{process.StartInfo.FileName} {process.StartInfo.Arguments}".Trim();

            return new WinGetRunnerResult(processCall, consoleOutput, process.ExitCode);
        }
    }
}
