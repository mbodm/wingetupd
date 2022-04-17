using WinGetUpdLogging;

namespace WinGetUpdCore
{
    public sealed class WinGetWrapper : IWinGetWrapper
    {
        private readonly IWinGetRunner winGetRunner;
        private readonly ILogger logger;

        public WinGetWrapper(IWinGetRunner winGetRunner, ILogger logger)
        {
            this.winGetRunner = winGetRunner ?? throw new ArgumentNullException(nameof(winGetRunner));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task WinGetWarmup(CancellationToken cancellationToken)
        {
            await winGetRunner.RunWinGetAsync("search", string.Empty, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> SearchPackageAsync(string package, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(package))
            {
                throw new ArgumentException($"'{nameof(package)}' cannot be null or whitespace.", nameof(package));
            }

            var result = await winGetRunner.RunWinGetAsync("search", $"--exact --id {package}", cancellationToken).ConfigureAwait(false);

            Console.WriteLine("--- SEARCH anfang ---");
            Console.WriteLine(result.ProcessCall);
            Console.WriteLine(result.ConsoleOutput);
            Console.WriteLine("--- SEARCH ende ---");
            Console.WriteLine();

            return result.ExitCode == 0 && result.ConsoleOutput.Contains(package);
        }

        public async Task<WinGetWrapperListResult> ListPackageAsync(string package, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(package))
            {
                throw new ArgumentException($"'{nameof(package)}' cannot be null or whitespace.", nameof(package));
            }

            var result = await winGetRunner.RunWinGetAsync("list", $"--exact --id {package}", cancellationToken).ConfigureAwait(false);

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

            Console.WriteLine("--- LIST anfang ---");
            Console.WriteLine(result.ProcessCall);
            Console.WriteLine(result.ConsoleOutput);
            Console.WriteLine("--- LIST ende ---");
            Console.WriteLine();

            return new WinGetWrapperListResult(package, installed, updatable);
        }

        public async Task<bool> UpgradePackageAsync(string package, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(package))
            {
                throw new ArgumentException($"'{nameof(package)}' cannot be null or whitespace.", nameof(package));
            }

            var result = await winGetRunner.RunWinGetAsync("upgrade", $"--exact --id {package}", cancellationToken).ConfigureAwait(false);

            return result.ExitCode == 0;
        }
    }
}
