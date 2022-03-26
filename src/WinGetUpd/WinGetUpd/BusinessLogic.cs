using System.ComponentModel;
using System.Diagnostics;

namespace WinGetUpd
{
    internal sealed class BusinessLogic
    {
        private readonly IWinGet winGet;

        public BusinessLogic(IWinGet winGet)
        {
            this.winGet = winGet ?? throw new ArgumentNullException(nameof(winGet));
        }

        public async Task<bool> WinGetExists()
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = "--version",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                    }
                };

                process.Start();

                await process.WaitForExitAsync().ConfigureAwait(false);

                return true;
            }
            catch (Win32Exception)
            {
                return false;
            }
        }

        public bool PackageFileExists()
        {
            return File.Exists($"{AppData.PkgFile}");
        }

        public async Task<IEnumerable<string>> GetPackages()
        {
            var lines = await File.ReadAllLinesAsync(AppData.PkgFile).ConfigureAwait(false);

            return lines;
        }

        public async Task ProcessPackages(IEnumerable<string> packages, IProgress<ProgressData> progress)
        {
            var tasks = packages.Select(package => ProcessPackage(package, progress)).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task ProcessPackage(string package, IProgress<ProgressData> progress)
        {
            var packageExists = await winGet.PackageExistsAsync(package).ConfigureAwait(false);
            if (!packageExists)
            {
                var error = $"The package '{package}', given in package-file ('{AppData.PkgFile}'), not exists.";
                progress.Report(new ProgressData { Package = package, Status = ProgressStatus.ErrorOccurred, Error = error });

                return;
            }

            progress.Report(new ProgressData { Package = package, Status = ProgressStatus.PackageExists });

            var packageIsInstalled = await winGet.PackageIsInstalledAsync(package).ConfigureAwait(false);
            if (!packageIsInstalled)
            {
                var error = $"The package '{package}', given in package-file ('{AppData.PkgFile}'), is not installed.";
                progress.Report(new ProgressData { Package = package, Status = ProgressStatus.ErrorOccurred, Error = error });

                return;
            }

            progress.Report(new ProgressData { Package = package, Status = ProgressStatus.PackageIsInstalled });

            var packageHasUpdate = await winGet.PackageHasUpdateAsync(package).ConfigureAwait(false);
            if (packageHasUpdate)
            {
                await winGet.UpdatePackageAsync(package).ConfigureAwait(false);
                progress.Report(new ProgressData { Package = package, Status = ProgressStatus.PackageUpdated });
            }
        }
    }
}
