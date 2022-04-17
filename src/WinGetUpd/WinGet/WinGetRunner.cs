using System.Diagnostics;
using System.Text;

namespace WinGet
{
    public sealed class WinGetRunner : IWinGetRunner
    {
        private const string WinGetApp = "winget.exe";
        private const double WinGetAppTimeoutInSeconds = 30;

        public async Task<WinGetRunnerResult> RunWinGetAsync(string command, string options, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException($"'{nameof(command)}' cannot be null or whitespace.", nameof(command));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            command = command.Trim();
            options = options.Trim();

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = WinGetApp,
                Arguments = options != string.Empty ? $"{command} {options}" : command,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8
            });

            if (process == null)
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

            return new WinGetRunnerResult($"{process.StartInfo.FileName} {process.StartInfo.Arguments}", consoleOutput, process.ExitCode);
        }
    }
}
