// The word 'package' is used synonym for 'WinGet package ID' in the whole project,
// since everything package-related will always be based on WinGet package ID here.

using WinGetUpdConfig;
using WinGetUpdExecution;
using WinGetUpdLogging;
using WinGetUpdPackages;

namespace WinGetUpdCore
{
    public sealed class BusinessLogic
    {
        private bool isInitialized;

        private readonly IWinGet winGet;
        private readonly IFileLogger fileLogger;
        private readonly IPackageManager packageManager;
        private readonly IPackageFileReader packageFileReader;

        public BusinessLogic(IWinGet winGet, IFileLogger fileLogger, IPackageManager packageManager, IPackageFileReader packageFileReader)
        {
            this.winGet = winGet ?? throw new ArgumentNullException(nameof(winGet));
            this.fileLogger = fileLogger ?? throw new ArgumentNullException(nameof(fileLogger));
            this.packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            this.packageFileReader = packageFileReader ?? throw new ArgumentNullException(nameof(packageFileReader));
        }

        /// <summary>
        /// Asynchronously initializes BusinessLogic (call is reentrant).
        /// </summary>
        /// <param name="writeLogFile">A boolean value, indicating if this module will create a log file.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task.</returns>
        /// <exception cref="BusinessLogicException"></exception>
        public async Task InitAsync(bool writeLogFile, CancellationToken cancellationToken = default)
        {
            if (!isInitialized)
            {
                if (!winGet.WinGetIsInstalled)
                {
                    throw new BusinessLogicException("It seems WinGet is not installed on this machine.");
                }

                if (writeLogFile)
                {
                    if (!await fileLogger.CanWriteLogFileAsync(cancellationToken).ConfigureAwait(false))
                    {
                        var logFile = Path.GetFileName(fileLogger.LogFile);

                        throw new BusinessLogicException($"Can not create log file ('{logFile}'). It seems this folder has no write permissions.");
                    }
                }

                packageManager.LogWinGetCalls = writeLogFile;

                if (!packageFileReader.PackageFileExists)
                {
                    var packageFile = Path.GetFileName(packageFileReader.PackageFile);

                    throw new BusinessLogicException($"The package-file ('{packageFile}') not exists.");
                }

                isInitialized = true;
            }
        }

        /// <summary>
        /// Asynchronously gets entries from package file.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the WinGet package id entries from package file.</returns>
        public async Task<IEnumerable<string>> GetPackageFileEntriesAsync(CancellationToken cancellationToken = default)
        {
            var entries = await packageFileReader.ReadPackageFileAsync(cancellationToken).ConfigureAwait(false);

            if (!entries.Any())
            {
                throw new BusinessLogicException($"The package-file is empty.");
            }

            return entries;
        }

        /// <summary>
        /// Asynchronously analyzes every package in a list of given packages.
        /// </summary>
        /// <param name="packages">A list of WinGet package id´s.</param>
        /// <param name="progress">The typical TAP-Pattern progress handler, to get notified about every single step.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents a list of package infos, containing one package info for every analyzed package.</returns>
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

            if (!isInitialized)
            {
                throw new BusinessLogicException($"{nameof(BusinessLogic)} not initialized.");
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
        /// Asynchronously updates every WinGet package that is listed as 'updatable' in a given list of package infos.
        /// </summary>
        /// <param name="packageInfos">The list of package infos, returned by <see cref="AnalyzePackagesAsync"/> method.</param>
        /// <param name="progress">The typical TAP-Pattern progress handler, to get notified about every single step.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents a list of WinGet package id´s, containing one WinGet package id for every updated WinGet package.</returns>
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

            if (!isInitialized)
            {
                throw new BusinessLogicException($"{nameof(BusinessLogic)} not initialized.");
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
            var valid = await packageManager.SearchPackageAsync(package, cancellationToken).ConfigureAwait(false);
            if (!valid)
            {
                Report(progress, package, PackageProgressStatus.PackageNotValid);

                return NotValid(package);
            }

            Report(progress, package, PackageProgressStatus.PackageValid);

            var listResult = await packageManager.ListPackageAsync(package, cancellationToken).ConfigureAwait(false);
            if (!listResult.IsInstalled)
            {
                Report(progress, package, PackageProgressStatus.PackageNotInstalled);

                return ValidButNotInstalled(package);
            }

            Report(progress, package, PackageProgressStatus.PackageInstalled);

            if (!listResult.IsUpdatable)
            {
                Report(progress, package, PackageProgressStatus.PackageNotUpdatable);

                return ValidInstalledButNotUpdatable(package, listResult.InstalledVersion);
            }

            Report(progress, package, PackageProgressStatus.PackageUpdatable);

            return ValidInstalledAndUpdatable(package, listResult.InstalledVersion, listResult.UpdateVersion);
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

            var updated = await packageManager.UpgradePackageAsync(packageInfo.Package, cancellationToken).ConfigureAwait(false);
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
            new(package, false, false, false, string.Empty, string.Empty);

        private static PackageInfo ValidButNotInstalled(string package) =>
            new(package, true, false, false, string.Empty, string.Empty);

        private static PackageInfo ValidInstalledButNotUpdatable(string package, string installedVersion) =>
            new(package, true, true, false, installedVersion, string.Empty);

        private static PackageInfo ValidInstalledAndUpdatable(string package, string installedVersion, string updateVersion) =>
            new(package, true, true, true, installedVersion, updateVersion);

        private static (string package, bool updated) NotUpdated(PackageInfo packageInfo) =>
            (packageInfo.Package, false);

        private static (string package, bool updated) Updated(PackageInfo packageInfo) =>
            (packageInfo.Package, true);
    }
}
