using System.Diagnostics;

namespace WinGetUpd
{
    internal sealed class WinGet : IWinGet
    {
        private readonly IWinGetLogger winGetLogger;

        public WinGet(IWinGetLogger winGetLogger)
        {
            this.winGetLogger = winGetLogger ?? throw new ArgumentNullException(nameof(WinGet.winGetLogger));
        }

        public async Task<bool> PackageExistsAsync(string id)
        {
            var output = await RunWinGetCommandAsync("search", id).ConfigureAwait(false);

            return output.Contains(id);
        }

        public async Task<bool> PackageIsInstalledAsync(string id)
        {
            var output = await RunWinGetCommandAsync("list", id).ConfigureAwait(false);

            return output.Contains(id);
        }

        public async Task<bool> PackageHasUpdateAsync(string id)
        {
            var output = await RunWinGetCommandAsync("list", id).ConfigureAwait(false);

            return output.Contains(" Available ") || output.Contains(" Verf├╝gbar ");
        }

        public Task UpdatePackageAsync(string id)
        {
            return RunWinGetCommandAsync("upgrade", id);
        }

        private async Task<string> RunWinGetCommandAsync(string command, string id)
        {
            var arguments = $"{command} --exact --id {id}";

            var output = await StartWinGetProcessAsync(arguments).ConfigureAwait(false);

            await winGetLogger.LogAsync($"winget.exe {arguments}", output).ConfigureAwait(false);

            return output;
        }

        private static async Task<string> StartWinGetProcessAsync(string arguments)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "winget",
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };

            process.Start();

            await process.WaitForExitAsync().ConfigureAwait(false);

            var output = process.StandardOutput.ReadToEnd();

            return output;
        }
    }
}
