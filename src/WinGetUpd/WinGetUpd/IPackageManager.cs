namespace WinGetUpd
{
    internal interface IPackageManager
    {
        Task<IEnumerable<string>> ShowUpdatablePackagesAsync(
            IEnumerable<string> packages,
            IProgress<PackageProgressData>? progress = default,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<string>> UpdatePackagesAsync(
            IEnumerable<string> packages,
            IProgress<PackageProgressData>? progress = default,
            CancellationToken cancellationToken = default);
    }
}
