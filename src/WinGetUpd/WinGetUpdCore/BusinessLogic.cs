using WinGet;

namespace WinGetUpdCore
{
    public sealed class BusinessLogic : IBusinessLogic
    {
        private bool isInitialized;

        private readonly IWinGetRunner winGetRunner;
        private readonly IWinGetManager winGetManager;

        public BusinessLogic(IWinGetRunner winGetRunner, IWinGetManager winGetManager)
        {
            this.winGetRunner = winGetRunner ?? throw new ArgumentNullException(nameof(winGetRunner));
            this.winGetManager = winGetManager ?? throw new ArgumentNullException(nameof(winGetManager));
        }

        public void InitAsync(CancellationToken cancellationToken = default)
        {
            if (!isInitialized)
            {
                isInitialized = true;

                if (!File.Exists($"{AppData.PkgFile}"))
                {
                    throw new BusinessLogicException($"The package-file ('{AppData.PkgFile}') not exists.");
                }

                //if (!await prerequisitesHelper.CanWriteLogFileAsync(cancellationToken).ConfigureAwait(false))
                //{
                //    throw new BusinessLogicException($"Can not create log file ('{AppData.LogFile}'). It seems this folder has no write permissions.");
                //}

                if (!winGetRunner.WinGetIsInstalled)
                {
                    throw new BusinessLogicException("It seems WinGet is not installed on this computer.");
                }
            }
        }

        public async Task<IEnumerable<string>> GetPackageFileEntries(CancellationToken cancellationToken = default)
        {
            var entries = await File.ReadAllLinesAsync(AppData.PkgFile, cancellationToken).ConfigureAwait(false);
            
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
                throw new InvalidOperationException("Given list of packages is empty.");
            }

            if (packages.Any(package => string.IsNullOrWhiteSpace(package)))
            {
                throw new InvalidOperationException("Given list of packages contains null or empty entries.");
            }

            // Todo: Comment why we can not use concurrent version with Task.WhenAll() here.

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
                throw new InvalidOperationException("Given list of package infos is empty.");
            }

            var tasks = packageInfos.Select(packageInfo => UpdatePackageAsync(packageInfo, progress, cancellationToken)).ToList();

            var tuples = await Task.WhenAll(tasks).ConfigureAwait(false);

            var updatedPackages = tuples.Where(tuple => tuple.updated).Select(tuple => tuple.package);

            return updatedPackages;
        }

        private async Task<PackageInfo> AnalyzePackageAsync(
            string package,
            IProgress<PackageProgressData>? progress,
            CancellationToken cancellationToken)
        {
            var valid = await winGetWrapper.SearchPackageAsync(package, cancellationToken).ConfigureAwait(false);
            if (!valid)
            {
                Report(progress, package, PackageProgressStatus.PackageNotValid);

                return NotValid(package);
            }

            Report(progress, package, PackageProgressStatus.PackageValid);

            var listResult = await winGetWrapper.ListPackageAsync(package, cancellationToken).ConfigureAwait(false);
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

                return (packageInfo.Package, false);
            }

            Report(progress, packageInfo.Package, PackageProgressStatus.PackageValid);

            if (!packageInfo.IsInstalled)
            {
                Report(progress, packageInfo.Package, PackageProgressStatus.PackageNotInstalled);

                return (packageInfo.Package, false);
            }

            Report(progress, packageInfo.Package, PackageProgressStatus.PackageInstalled);

            if (!packageInfo.IsUpdatable)
            {
                Report(progress, packageInfo.Package, PackageProgressStatus.PackageNotUpdatable);

                return (packageInfo.Package, false);
            }

            Report(progress, packageInfo.Package, PackageProgressStatus.PackageUpdatable);

            var updated = await winGetWrapper.UpgradePackageAsync(packageInfo.Package, cancellationToken).ConfigureAwait(false);
            if (!updated)
            {
                Report(progress, packageInfo.Package, PackageProgressStatus.PackageNotUpdated);

                return (packageInfo.Package, false);
            }

            Report(progress, packageInfo.Package, PackageProgressStatus.PackageUpdated);

            return (packageInfo.Package, true);
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
    }
}
