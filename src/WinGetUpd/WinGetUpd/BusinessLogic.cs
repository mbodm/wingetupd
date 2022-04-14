using System.Collections.Concurrent;

namespace WinGetUpd
{
    internal sealed class BusinessLogic
    {
        private readonly IPrerequisitesValidator prerequisitesValidator;
        private readonly IPackageManager packageManager;

        public BusinessLogic(IPrerequisitesValidator prerequisitesValidator, IPackageManager packageManager)
        {
            this.prerequisitesValidator = prerequisitesValidator ?? throw new ArgumentNullException(nameof(prerequisitesValidator));
            this.packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
        }

        public IPackageManager PackageManager => packageManager;

        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (!prerequisitesValidator.PackageFileExists())
            {
                throw new InvalidOperationException($"The package-file ('{AppData.PkgFile}') not exists.");
            }

            if (!await prerequisitesValidator.CanWriteLogFileAsync(cancellationToken))
            {
                throw new InvalidOperationException($"Can not create log file ('{AppData.LogFile}'). It seems this folder has no write permissions.");
            }

            if (!await prerequisitesValidator.WinGetExistsAsync(cancellationToken))
            {
                throw new InvalidOperationException("It seems WinGet is not installed on this computer.");
            }
        }

        public async Task<IEnumerable<string>> GetPackagesAsync(CancellationToken cancellationToken = default)
        {
            var lines = await File.ReadAllLinesAsync(AppData.PkgFile, cancellationToken);

            return lines;
        }

        public async Task<BusinessLogicResult> GetSummaryAsync(IEnumerable<string> packages, IProgress<PackageProgressData>? progress = default, CancellationToken cancellationToken = default)
        {
            var existingPackages = new ConcurrentBag<string>();
            var nonExistingPackages = new ConcurrentBag<string>();
            var installedPackages = new ConcurrentBag<string>();
            var nonInstalledPackages = new ConcurrentBag<string>();
            var updatablePackages = new ConcurrentBag<string>();
            var nonUpdatablePackages = new ConcurrentBag<string>();
            var updatedPackages = new ConcurrentBag<string>();
            var nonUpdatedPackages = new ConcurrentBag<string>();

            var packageProgress = new PackageProgress(packageProgressData =>
            {
                switch (packageProgressData.Status)
                {
                    case PackageProgressStatus.PackageExists:
                        existingPackages.Add(packageProgressData.Package);
                        break;
                    case PackageProgressStatus.PackageNotExists:
                        nonExistingPackages.Add(packageProgressData.Package);
                        throw new InvalidOperationException("Package not exists. Stoped.");
                        break;
                    case PackageProgressStatus.PackageInstalled:
                        installedPackages.Add(packageProgressData.Package);
                        break;
                    case PackageProgressStatus.PackageNotInstalled:
                        nonInstalledPackages.Add(packageProgressData.Package);
                        break;
                    case PackageProgressStatus.PackageUpdatable:
                        updatablePackages.Add(packageProgressData.Package);
                        break;
                    case PackageProgressStatus.PackageNotUpdatable:
                        nonUpdatablePackages.Add(packageProgressData.Package);
                        break;
                    case PackageProgressStatus.PackageUpdated:
                        updatedPackages.Add(packageProgressData.Package);
                        break;
                    case PackageProgressStatus.PackageNotUpdated:
                        nonUpdatablePackages.Add(packageProgressData.Package);
                        break;
                }

                progress?.Report(packageProgressData);
            });

            await packageManager.ShowUpdatablePackagesAsync(packages, packageProgress, cancellationToken);

            return new BusinessLogicResult(
                packages,
                existingPackages,
                nonExistingPackages,
                installedPackages,
                nonInstalledPackages,
                updatablePackages,
                nonUpdatablePackages,
                updatedPackages,
                nonUpdatedPackages);
        }
    }
}
