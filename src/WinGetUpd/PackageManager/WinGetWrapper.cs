using WinGet;

namespace PackageManager
{
    public sealed class WinGetWrapper : IWinGetWrapper
    {
        private readonly IWinGet winGet;

        public WinGetWrapper(IWinGet winGet)
        {
            this.winGet = winGet ?? throw new ArgumentNullException(nameof(winGet));
        }

        public async Task<bool> SearchPackageAsync(string package, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(package))
            {
                throw new ArgumentException($"'{nameof(package)}' cannot be null or whitespace.", nameof(package));
            }

            var result = await winGet.RunWinGetAsync("search", $"--exact --id {package}", cancellationToken);

            return result.ExitCode == 0 && result.ConsoleOutput.Contains(package);
        }

        public async Task<WinGetWrapperListResult> ListPackageAsync(string package, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(package))
            {
                throw new ArgumentException($"'{nameof(package)}' cannot be null or whitespace.", nameof(package));
            }

            var result = await winGet.RunWinGetAsync("list", $"--exact --id {package}", cancellationToken);

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

            return new WinGetWrapperListResult(package, installed, updatable);
        }

        public async Task<bool> UpgradePackageAsync(string package, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(package))
            {
                throw new ArgumentException($"'{nameof(package)}' cannot be null or whitespace.", nameof(package));
            }

            var result = await winGet.RunWinGetAsync("upgrade", $"--exact --id {package}", cancellationToken);

            return result.ExitCode == 0;
        }
    }
}
