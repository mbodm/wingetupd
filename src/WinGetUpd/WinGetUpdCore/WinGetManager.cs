using WinGet;
using WinGetUpdLogging;

namespace WinGetUpdCore
{
    public sealed class WinGetManager : IWinGetManager
    {
        private readonly IWinGetRunner winGetRunner;
        private readonly IFileLogger fileLogger;

        public WinGetManager(IWinGetRunner winGetRunner, IFileLogger fileLogger)
        {
            this.winGetRunner = winGetRunner ?? throw new ArgumentNullException(nameof(winGetRunner));
            this.fileLogger = fileLogger ?? throw new ArgumentNullException(nameof(fileLogger));
        }

        public bool LogWinGetCalls { get; set; } = true;

        public async Task<bool> SearchPackageAsync(string package, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(package))
            {
                throw new ArgumentException($"'{nameof(package)}' cannot be null or whitespace.", nameof(package));
            }

            var result = await winGetRunner.RunWinGetAsync($"search --exact --id {package}", cancellationToken).ConfigureAwait(false);

            if (LogWinGetCalls)
            {
                await fileLogger.LogWinGetCallAsync(result.ProcessCall, result.ConsoleOutput, cancellationToken).ConfigureAwait(false);
            }

            return result.ExitCode == 0 && result.ConsoleOutput.Contains(package);
        }

        public async Task<WinGetManagerListResult> ListPackageAsync(string package, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(package))
            {
                throw new ArgumentException($"'{nameof(package)}' cannot be null or whitespace.", nameof(package));
            }

            var result = await winGetRunner.RunWinGetAsync($"list --exact --id {package}", cancellationToken).ConfigureAwait(false);

            if (LogWinGetCalls)
            {
                await fileLogger.LogWinGetCallAsync(result.ProcessCall, result.ConsoleOutput, cancellationToken).ConfigureAwait(false);
            }

            var installed = false;
            var updatable = false;

            if (result.ExitCode == 0 && result.ConsoleOutput.Contains(package))
            {
                installed = true;

                if (result.ConsoleOutput.Contains(" Available ") || result.ConsoleOutput.Contains(" Verfügbar "))
                {
                    updatable = true;
                }
            }

            return new WinGetManagerListResult(package, installed, updatable);
        }

        public async Task<bool> UpgradePackageAsync(string package, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(package))
            {
                throw new ArgumentException($"'{nameof(package)}' cannot be null or whitespace.", nameof(package));
            }

            // Since some software/packages can take a good amount of time while updating, timeout is adjusted accordingly here.

            var timeout = TimeSpan.FromMinutes(30);

            var result = await winGetRunner.RunWinGetAsync($"upgrade --exact --id {package}", timeout, cancellationToken).ConfigureAwait(false);

            if (LogWinGetCalls)
            {
                await fileLogger.LogWinGetCallAsync(result.ProcessCall, result.ConsoleOutput, cancellationToken).ConfigureAwait(false);
            }

            return result.ExitCode == 0;
        }
    }
}
