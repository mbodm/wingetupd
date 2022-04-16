using System.Diagnostics;
using System.Text;

namespace WinGet
{
    public sealed class WinGetRunner : IWinGetRunner
    {
        private const string WinGetApp = "winget.exe";
        private const double WinGetAppTimeoutInSeconds = 30;

        private readonly IWinGetLogger winGetLogger;

        public WinGetRunner(IWinGetLogger winGetLogger)
        {
            this.winGetLogger = winGetLogger ?? throw new ArgumentNullException(nameof(winGetLogger));
        }

        public async Task<WinGetRunnerResult> RunWinGetAsync(string command, string options, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException($"'{nameof(command)}' cannot be null or whitespace.", nameof(command));
            }

            if (string.IsNullOrWhiteSpace(options))
            {
                throw new ArgumentException($"'{nameof(options)}' cannot be null or whitespace.", nameof(options));
            }

            using var ctsTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(WinGetAppTimeoutInSeconds));
            using var ctsLinked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, ctsTimeout.Token);

            try
            {
                var result = await StartProcessAsync($"{command} {options}", ctsLinked.Token).ConfigureAwait(false);

                await winGetLogger.LogAsync($"{WinGetApp} {command} {options}", result.ConsoleOutput, cancellationToken).ConfigureAwait(false);

                return result;
            }
            catch (OperationCanceledException) when (ctsTimeout.IsCancellationRequested)
            {
                var output = $"{WinGetApp} reached timeout after {WinGetAppTimeoutInSeconds} seconds. {WinGetApp} process canceled.";

                await winGetLogger.LogAsync($"{WinGetApp} {command} {options}", output, cancellationToken).ConfigureAwait(false);

                throw;
            }
        }

        private static async Task<WinGetRunnerResult> StartProcessAsync(string arguments, CancellationToken cancellationToken = default)
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = WinGetApp,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8
            });

            if (process == null)
            {
                throw new InvalidOperationException($"Could not start {WinGetApp} process.");
            }

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            var stdOut = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
            var stdErr = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);

            var consoleOutput = string.Empty;

            if (!string.IsNullOrWhiteSpace(stdOut) && !string.IsNullOrWhiteSpace(stdErr))
            {
                consoleOutput = stdOut + Environment.NewLine + stdErr;
            }
            else if (!string.IsNullOrWhiteSpace(stdOut))
            {
                consoleOutput = stdOut;
            }
            else if (!string.IsNullOrWhiteSpace(stdErr))
            {
                consoleOutput = stdErr;
            }
            else
            {
                consoleOutput = string.Empty;
            }

            return new WinGetRunnerResult($"{process.StartInfo.FileName} {process.StartInfo.Arguments}", consoleOutput, process.ExitCode);
        }
    }
}
