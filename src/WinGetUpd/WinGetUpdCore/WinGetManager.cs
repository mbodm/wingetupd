using WinGet;

namespace WinGetUpdCore
{
    public sealed class WinGetManager : IWinGetManager
    {
        private readonly IWinGetRunner winGetRunner;

        public WinGetManager(IWinGetRunner winGetRunner)
        {
            this.winGetRunner = winGetRunner ?? throw new ArgumentNullException(nameof(winGetRunner));
        }

        public async Task<bool> SearchPackageAsync(string package, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(package))
            {
                throw new ArgumentException($"'{nameof(package)}' cannot be null or whitespace.", nameof(package));
            }

            var result = await winGetRunner.RunWinGetAsync($"search --exact --id {package}", cancellationToken).ConfigureAwait(false);

            return result.ExitCode == 0 && result.ConsoleOutput.Contains(package);
        }

        public async Task<WinGetManagerListResult> ListPackageAsync(string package, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(package))
            {
                throw new ArgumentException($"'{nameof(package)}' cannot be null or whitespace.", nameof(package));
            }

            var result = await winGetRunner.RunWinGetAsync($"list --exact --id {package}", cancellationToken).ConfigureAwait(false);

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

            var result = await winGetRunner.RunWinGetAsync($"upgrade --exact --id {package}", cancellationToken).ConfigureAwait(false);

            return result.ExitCode == 0;
        }
    }
}
