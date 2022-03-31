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

        public async Task<string> InitAsync()
        {
            if (!await WinGetExists())
            {
                return "Error: It seems WinGet is not installed on this computer.";
            }

            if (!PackageFileExists())
            {
                return $"Error: The package-file ('{AppData.PkgFile}') not exists.";
            }

            if (!CanWriteLogFile())
            {
                return $"Error: Can not create log file ('{AppData.LogFile}'). It seems this folder has no write permissions.";
            }

            return string.Empty;
        }

        public async Task<IEnumerable<string>> GetPackagesAsync()
        {
            var lines = await File.ReadAllLinesAsync(AppData.PkgFile);

            return lines;
        }

        public async Task ProcessPackagesAsync(IEnumerable<string> packages, IProgress<ProgressData> progress)
        {
            var tasks = packages.Select(package => ProcessPackage(package, progress)).ToList();

            await Task.WhenAll(tasks);
        }

        private async Task<bool> WinGetExists()
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

                await process.WaitForExitAsync();

                return true;
            }
            catch (Win32Exception)
            {
                return false;
            }
        }

        private bool PackageFileExists()
        {
            return File.Exists($"{AppData.PkgFile}");
        }

        private bool CanWriteLogFile()
        {
            try
            {
                File.WriteAllText(AppData.LogFile, string.Empty);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private async Task ProcessPackage(string package, IProgress<ProgressData> progress)
        {
            var packageExists = await winGet.PackageExistsAsync(package);
            if (!packageExists)
            {
                var error = $"The package '{package}', given in package-file ('{AppData.PkgFile}'), not exists.";
                progress.Report(new ProgressData { Package = package, Status = ProgressStatus.ErrorOccurred, Error = error });

                return;
            }

            progress.Report(new ProgressData { Package = package, Status = ProgressStatus.PackageExists });

            var packageIsInstalled = await winGet.PackageIsInstalledAsync(package);
            if (!packageIsInstalled)
            {
                var error = $"The package '{package}', given in package-file ('{AppData.PkgFile}'), is not installed.";
                progress.Report(new ProgressData { Package = package, Status = ProgressStatus.ErrorOccurred, Error = error });

                return;
            }

            progress.Report(new ProgressData { Package = package, Status = ProgressStatus.PackageIsInstalled });

            var packageHasUpdate = await winGet.PackageHasUpdateAsync(package);
            if (packageHasUpdate)
            {
                await winGet.UpdatePackageAsync(package);
                progress.Report(new ProgressData { Package = package, Status = ProgressStatus.PackageUpdated });
            }
        }
    }
}
