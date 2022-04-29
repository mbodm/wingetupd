using WinGet;
using WinGetUpdLogging;

// The word 'package' is used synonym for 'WinGet package ID' in the whole project,
// since everything package-related will always be based on WinGet package ID here.

namespace WinGetUpdCore
{
    public sealed class BusinessLogic
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

        /// <summary>
        /// Contains common application informations
        /// </summary>
        public static ApplicationData AppData { get; } = new ApplicationData();

        /// <summary>
        /// Initializes BusinessLogic (call is reentrant)
        /// </summary>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns></returns>
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
                    throw new BusinessLogicException("It seems WinGet is not installed on this machine.");
                }
            }
        }

        /// <summary>
        /// Get entries from package file
        /// </summary>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns>List of WinGet package id entries from package file</returns>
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

        /// <summary>
        /// Analyzes every package in a list of given packages
        /// </summary>
        /// <param name="packages">List of WinGet package id´s</param>
        /// <param name="progress">Typical TAP progress handler pattern for progressing every step</param>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns>List of package infos (containing a package info for every analyzed package)</returns>
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

        /// <summary>
        /// Updates every WinGet package on computer listed as 'updatable' in a given list of package infos
        /// </summary>
        /// <param name="packageInfos">List of package infos returned by AnalyzePackagesAsync() method</param>
        /// <param name="progress">Typical TAP progress handler pattern for progressing every step</param>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns>List of WinGet package id´s (containing a WinGet package id for every updated WinGet package)</returns>
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

        private static void Report(IProgress<PackageProgressData>? progress, string package, PackageProgressStatus status) =>
            progress?.Report(new PackageProgressData(package, status));

        private static PackageInfo NotValid(string package) =>
            new(package, false, false, false);

        private static PackageInfo ValidButNotInstalled(string package) =>
            new(package, true, false, false);

        private static PackageInfo ValidInstalledButNotUpdatable(string package) =>
            new(package, true, true, false);

        private static PackageInfo ValidInstalledAndUpdatable(string package) =>
            new(package, true, true, true);

        private static (string package, bool updated) NotUpdated(PackageInfo packageInfo) =>
            (packageInfo.Package, false);

        private static (string package, bool updated) Updated(PackageInfo packageInfo) =>
            (packageInfo.Package, true);
    }
}
