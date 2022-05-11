using WinGetUpdExecution;
using WinGetUpdLogging;

namespace WinGetUpdPackages
{
    public sealed class PackageManager : IPackageManager
    {
        private readonly IWinGet winGet;
        private readonly IFileLogger fileLogger;

        public PackageManager(IWinGet winGet, IFileLogger fileLogger)
        {
            this.winGet = winGet ?? throw new ArgumentNullException(nameof(winGet));
            this.fileLogger = fileLogger ?? throw new ArgumentNullException(nameof(fileLogger));
        }

        public bool LogWinGetCalls { get; set; } = true;

        public async Task<bool> SearchPackageAsync(string package, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(package))
            {
                throw new ArgumentException($"'{nameof(package)}' cannot be null or whitespace.", nameof(package));
            }

            var result = await winGet.RunWinGetAsync($"search --exact --id {package}", cancellationToken).ConfigureAwait(false);

            if (LogWinGetCalls)
            {
                await fileLogger.LogWinGetCallAsync(result.ProcessCall, result.ConsoleOutput, cancellationToken).ConfigureAwait(false);
            }

            return result.ExitCode == 0 && result.ConsoleOutput.Contains(package);
        }

        public async Task<PackageManagerListResult> ListPackageAsync(string package, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(package))
            {
                throw new ArgumentException($"'{nameof(package)}' cannot be null or whitespace.", nameof(package));
            }

            var result = await winGet.RunWinGetAsync($"list --exact --id {package}", cancellationToken).ConfigureAwait(false);

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

            return new PackageManagerListResult(package, installed, updatable);
        }

        public async Task<bool> UpgradePackageAsync(string package, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(package))
            {
                throw new ArgumentException($"'{nameof(package)}' cannot be null or whitespace.", nameof(package));
            }

            // Since some software/packages can take a good amount of time while updating, timeout is adjusted accordingly here.

            var timeout = TimeSpan.FromMinutes(60);

            var result = await winGet.RunWinGetAsync($"upgrade --exact --id {package}", timeout, cancellationToken).ConfigureAwait(false);

            if (LogWinGetCalls)
            {
                await fileLogger.LogWinGetCallAsync(result.ProcessCall, result.ConsoleOutput, cancellationToken).ConfigureAwait(false);
            }

            return result.ExitCode == 0;
        }
    }
}
