using WinGet;
using WinGetUpdLogging;

namespace WinGetUpdCore
{
    public sealed class BusinessLogic : IBusinessLogic
    {
        private bool isInitialized;

        private readonly IFileLogger fileLogger;
        private readonly IWinGetRunner winGetRunner;
        private readonly IWinGetManager winGetManager;

        public BusinessLogic(IFileLogger fileLogger, IWinGetRunner winGetRunner, IWinGetManager winGetManager)
        {
            this.fileLogger = fileLogger ?? throw new ArgumentNullException(nameof(fileLogger));
            this.winGetRunner = winGetRunner ?? throw new ArgumentNullException(nameof(winGetRunner));
            this.winGetManager = winGetManager ?? throw new ArgumentNullException(nameof(winGetManager));
        }

        public static ApplicationData AppData { get; } = new ApplicationData();

        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (!isInitialized)
            {
                isInitialized = true;

                if (!File.Exists($"{AppData.PkgFilePath}"))
                {
                    throw new BusinessLogicException($"The package-file ('{AppData.PkgFileName}') not exists.");
                }

                if (!await fileLogger.CanWriteLogFileAsync(cancellationToken).ConfigureAwait(false))
                {
                    throw new BusinessLogicException($"Can not create log file ('{AppData.LogFileName}'). It seems this folder has no write permissions.");
                }

                if (!winGetRunner.WinGetIsInstalled)
                {
                    throw new BusinessLogicException("It seems WinGet is not installed on this computer.");
                }
            }
        }

        public async Task<IEnumerable<string>> GetPackageFileEntries(CancellationToken cancellationToken = default)
        {
            var entries = await File.ReadAllLinesAsync(AppData.PkgFilePath, cancellationToken).ConfigureAwait(false);

            var nonWhiteSpaceEntries = entries.Where(entry => !string.IsNullOrWhiteSpace(entry));

            if (!nonWhiteSpaceEntries.Any())
            {
                throw new BusinessLogicException("Package-File is empty.");
            }

            return nonWhiteSpaceEntries;
        }

        public async Task<IEnumerable<PackageInfo>> AnalyzePackagesAsync(
            IEnumerable<string> packages,
            IProgress<PackageProgressData>? progress = default,
            CancellationToken cancellationToken = default)
        {
            if (packages is null)
            {
                throw new ArgumentNullException(nameof(packages));
            }

            if (!packages.Any())
            {
                throw new ArgumentException("Given list of packages is empty.", nameof(packages));
            }

            if (packages.Any(package => string.IsNullOrWhiteSpace(package)))
            {
                throw new ArgumentException("Given list of packages contains null or empty entries.", nameof(packages));
            }

            // We can not use a concurrent logic here, by using some typical Task.WhenAll() approach. Because
            // WinGet fails with "Failed in attempting to update the source" errors, when running in parallel.
            // Therefore we sadly have to stick here with the non-concurrent, sequential, way slower approach.
            // Nonetheless, all parts and modules of this App are designed with a concurrent approach in mind.
            // So, if WinGet may change it´s behaviour in future, we are ready to use the concurrent approach.

            var packageInfos = new List<PackageInfo>();

            foreach (var package in packages)
            {
                var packageInfo = await AnalyzePackageAsync(package, progress, cancellationToken).ConfigureAwait(false);

                packageInfos.Add(packageInfo);
            }

            return packageInfos;
        }

        public async Task<IEnumerable<string>> UpdatePackagesAsync(
            IEnumerable<PackageInfo> packageInfos,
            IProgress<PackageProgressData>? progress = default,
            CancellationToken cancellationToken = default)
        {
            if (packageInfos is null)
            {
                throw new ArgumentNullException(nameof(packageInfos));
            }

            if (!packageInfos.Any())
            {
                throw new ArgumentException("Given list of package infos is empty.", nameof(packageInfos));
            }

            // We can not use a concurrent logic here, by using some typical Task.WhenAll() approach. Because
            // WinGet fails with "Failed in attempting to update the source" errors, when running in parallel.
            // Therefore we sadly have to stick here with the non-concurrent, sequential, way slower approach.
            // Nonetheless, all parts and modules of this App are designed with a concurrent approach in mind.
            // So, if WinGet may change it´s behaviour in future, we are ready to use the concurrent approach.

            var updatedPackages = new List<string>();

            foreach (var packageInfo in packageInfos)
            {
                var (package, updated) = await UpdatePackageAsync(packageInfo, progress, cancellationToken).ConfigureAwait(false);

                if (updated)
                {
                    updatedPackages.Add(package);
                }
            }

            return updatedPackages;
        }

        private async Task<PackageInfo> AnalyzePackageAsync(
            string package,
            IProgress<PackageProgressData>? progress,
            CancellationToken cancellationToken)
        {
            var valid = await winGetManager.SearchPackageAsync(package, cancellationToken).ConfigureAwait(false);
            if (!valid)
            {
                Report(progress, package, PackageProgressStatus.PackageNotValid);

                return NotValid(package);
            }

            Report(progress, package, PackageProgressStatus.PackageValid);

            var listResult = await winGetManager.ListPackageAsync(package, cancellationToken).ConfigureAwait(false);
            if (!listResult.IsInstalled)
            {
                Report(progress, package, PackageProgressStatus.PackageNotInstalled);

                return ValidButNotInstalled(package);
            }

            Report(progress, package, PackageProgressStatus.PackageInstalled);

            if (!listResult.IsUpdatable)
            {
                Report(progress, package, PackageProgressStatus.PackageNotUpdatable);

                return ValidInstalledButNotUpdatable(package);
            }

            Report(progress, package, PackageProgressStatus.PackageUpdatable);

            return ValidInstalledAndUpdatable(package);
        }

        private async Task<(string package, bool updated)> UpdatePackageAsync(
            PackageInfo packageInfo,
            IProgress<PackageProgressData>? progress,
            CancellationToken cancellationToken)
        {
            if (!packageInfo.IsValid)
            {
                Report(progress, packageInfo.Package, PackageProgressStatus.PackageNotValid);

                return NotUpdated(packageInfo);
            }

            Report(progress, packageInfo.Package, PackageProgressStatus.PackageValid);

            if (!packageInfo.IsInstalled)
            {
                Report(progress, packageInfo.Package, PackageProgressStatus.PackageNotInstalled);

                return NotUpdated(packageInfo);
            }

            Report(progress, packageInfo.Package, PackageProgressStatus.PackageInstalled);

            if (!packageInfo.IsUpdatable)
            {
                Report(progress, packageInfo.Package, PackageProgressStatus.PackageNotUpdatable);

                return NotUpdated(packageInfo);
            }

            Report(progress, packageInfo.Package, PackageProgressStatus.PackageUpdatable);

            var updated = await winGetManager.UpgradePackageAsync(packageInfo.Package, cancellationToken).ConfigureAwait(false);
            if (!updated)
            {
                Report(progress, packageInfo.Package, PackageProgressStatus.PackageNotUpdated);

                return NotUpdated(packageInfo);
            }

            Report(progress, packageInfo.Package, PackageProgressStatus.PackageUpdated);

            return Updated(packageInfo);
        }

        private static void Report(IProgress<PackageProgressData>? progress, string package, PackageProgressStatus status)
        {
            progress?.Report(new PackageProgressData(package, status));
        }

        private static PackageInfo NotValid(string package)
        {
            return new PackageInfo(package, false, false, false);
        }

        private static PackageInfo ValidButNotInstalled(string package)
        {
            return new PackageInfo(package, true, false, false);
        }

        private static PackageInfo ValidInstalledButNotUpdatable(string package)
        {
            return new PackageInfo(package, true, true, false);
        }

        private static PackageInfo ValidInstalledAndUpdatable(string package)
        {
            return new PackageInfo(package, true, true, true);
        }

        private static (string package, bool updated) NotUpdated(PackageInfo packageInfo)
        {
            return (packageInfo.Package, false);
        }

        private static (string package, bool updated) Updated(PackageInfo packageInfo)
        {
            return (packageInfo.Package, true);
        }
    }
}
