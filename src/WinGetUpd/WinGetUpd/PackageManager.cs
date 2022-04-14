namespace WinGetUpd
{
    internal sealed class PackageManager : IPackageManager
    {
        private readonly IWinGet winGet;

        public PackageManager(IWinGet winGet)
        {
            this.winGet = winGet ?? throw new ArgumentNullException(nameof(winGet));
        }

        public async Task<IEnumerable<string>> ShowUpdatablePackagesAsync(
            IEnumerable<string> packages,
            IProgress<PackageProgressData>? progress = default,
            CancellationToken cancellationToken = default)
        {
            if (packages is null)
            {
                throw new ArgumentNullException(nameof(packages));
            }

            ValidatePackages(packages);

            var tasks = packages.Select(package => ProcessPackageAsync(false, package, progress, cancellationToken)).ToList();

            var results = await Task.WhenAll(tasks);

            var updatablePackages = results.
                Where(result => result.Status == PackageProgressStatus.PackageUpdatable).
                Select(result => result.Package);

            return updatablePackages;
        }

        public async Task<IEnumerable<string>> UpdatePackagesAsync(
            IEnumerable<string> packages,
            IProgress<PackageProgressData>? progress = default,
            CancellationToken cancellationToken = default)
        {
            if (packages is null)
            {
                throw new ArgumentNullException(nameof(packages));
            }

            ValidatePackages(packages);

            var tasks = packages.Select(package => ProcessPackageAsync(true, package, progress, cancellationToken)).ToList();

            var results = await Task.WhenAll(tasks);

            var updatedPackages = results.
               Where(result => result.Status == PackageProgressStatus.PackageUpdated).
               Select(result => result.Package);

            return updatedPackages;
        }

        private static void ValidatePackages(IEnumerable<string> packages)
        {
            if (!packages.Any())
            {
                throw new InvalidOperationException("Given list of packages is empty.");
            }

            if (packages.Any(package => string.IsNullOrWhiteSpace(package)))
            {
                throw new InvalidOperationException("Given list of packages contains null or empty entries.");
            }
        }

        private async Task<PackageProgressData> ProcessPackageAsync(
            bool update,
            string package,
            IProgress<PackageProgressData>? progress,
            CancellationToken cancellationToken)
        {
            var exists = await PackageExistsAsync(package, cancellationToken);
            if (!exists)
            {
                return Report(progress, package, PackageProgressStatus.PackageNotExists);
            }

            Report(progress, package, PackageProgressStatus.PackageExists);

            var (installed, updatable) = await GetPackageInfoAsync(package, cancellationToken);
            if (!installed)
            {
                return Report(progress, package, PackageProgressStatus.PackageNotInstalled);
            }

            Report(progress, package, PackageProgressStatus.PackageInstalled);

            if (!updatable)
            {
                return Report(progress, package, PackageProgressStatus.PackageNotUpdatable);
            }

            var result = Report(progress, package, PackageProgressStatus.PackageUpdatable);

            if (!update)
            {
                return result;
            }

            var updated = await UpdatePackageAsync(package, cancellationToken);
            if (!updated)
            {
                return Report(progress, package, PackageProgressStatus.PackageNotUpdated);
            }

            return Report(progress, package, PackageProgressStatus.PackageUpdated);
        }

        private async Task<bool> PackageExistsAsync(string id, CancellationToken cancellationToken)
        {
            var result = await winGet.SearchAsync(id, cancellationToken);

            return result.ExitCode == 0 && result.ConsoleOutput.Contains(id);
        }

        private async Task<(bool installed, bool updatable)> GetPackageInfoAsync(string id, CancellationToken cancellationToken)
        {
            var result = await winGet.ListAsync(id, cancellationToken);

            var installed = false;
            var updatable = false;

            if (result.ExitCode == 0 && result.ConsoleOutput.Contains(id))
            {
                installed = true;

                if (result.ConsoleOutput.Contains(" Available ") || result.ConsoleOutput.Contains(" Verfügbar "))
                {
                    updatable = true;
                }
            }

            return (installed, updatable);
        }

        private async Task<bool> UpdatePackageAsync(string id, CancellationToken cancellationToken)
        {
            var result = await winGet.UpgradeAsync(id, cancellationToken);

            return result.ExitCode == 0;
        }

        private static PackageProgressData Report(IProgress<PackageProgressData>? progress, string package, PackageProgressStatus status)
        {
            var data = new PackageProgressData(package, status);

            progress?.Report(data);

            return data;
        }
    }
}
