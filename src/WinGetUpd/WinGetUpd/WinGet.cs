using System.Diagnostics;
using System.Text;

namespace WinGetUpd
{
    internal sealed class WinGet : IWinGet
    {
        private const string WinGetApp = "winget.exe";
        private const int WinGetAppTimeoutSeconds = 30;

        private readonly IWinGetLogger winGetLogger;

        public WinGet(IWinGetLogger winGetLogger)
        {
            this.winGetLogger = winGetLogger ?? throw new ArgumentNullException(nameof(WinGet.winGetLogger));
        }

        public Task<WinGetResult> SearchAsync(string id, CancellationToken cancellationToken = default)
        {
            return RunCommandAsync("search", id, cancellationToken);
        }

        public Task<WinGetResult> ListAsync(string id, CancellationToken cancellationToken = default)
        {
            return RunCommandAsync("list", id, cancellationToken);
        }

        public Task<WinGetResult> UpgradeAsync(string id, CancellationToken cancellationToken = default)
        {
            return RunCommandAsync("upgrade", id, cancellationToken);
        }

        private async Task<WinGetResult> RunCommandAsync(string command, string id, CancellationToken cancellationToken = default)
        {
            var arguments = $"{command} --exact --id {id}";

            using var ctsTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(WinGetAppTimeoutSeconds));
            using var ctsLinked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, ctsTimeout.Token);

            try
            {
                var result = await StartProcessAsync(arguments, ctsLinked.Token);

                await winGetLogger.LogAsync($"{WinGetApp} {arguments}", result.ConsoleOutput, cancellationToken);

                return result;
            }
            catch (OperationCanceledException) when (ctsTimeout.IsCancellationRequested)
            {
                var output = $"winget.exe reached timeout after {WinGetAppTimeoutSeconds} seconds. winget.exe process canceled.";

                await winGetLogger.LogAsync($"{WinGetApp} {arguments}", output, cancellationToken);

                throw;
            }
        }

        private static async Task<WinGetResult> StartProcessAsync(string arguments, CancellationToken cancellationToken = default)
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

            await process.WaitForExitAsync(cancellationToken);

            var stdOut = await process.StandardOutput.ReadToEndAsync();
            var stdErr = await process.StandardError.ReadToEndAsync();

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

            return new WinGetResult($"{process.StartInfo.FileName} {process.StartInfo.Arguments}", consoleOutput, process.ExitCode);
        }
    }
}
