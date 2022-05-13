using WinGetUpd.Execution;
using WinGetUpd.Logging;
using WinGetUpd.Parsing;

namespace WinGetUpd.Packages
{
    public sealed class PackageManager : IPackageManager
    {
        private readonly IWinGet winGet;
        private readonly IFileLogger fileLogger;
        private readonly IWinGetOutputParser winGetOutputParser;

        public PackageManager(IWinGet winGet, IFileLogger fileLogger, IWinGetOutputParser winGetOutputParser)
        {
            this.winGet = winGet ?? throw new ArgumentNullException(nameof(winGet));
            this.fileLogger = fileLogger ?? throw new ArgumentNullException(nameof(fileLogger));
            this.winGetOutputParser = winGetOutputParser ?? throw new ArgumentNullException(nameof(winGetOutputParser));
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

            if (result.ExitCode == 0 && result.ConsoleOutput.Contains(package))
            {
                // If we are here, the package is at least installed.

                var parsed = winGetOutputParser.ParseListOutput(result.ConsoleOutput);

                return new PackageManagerListResult(package, true, parsed.IsUpdatable, parsed.OldVersion, parsed.NewVersion);
            }

            return new PackageManagerListResult(package, false, false, string.Empty, string.Empty);
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
