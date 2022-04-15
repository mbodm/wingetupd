namespace PackageManager
{
    public interface IPackageManager
    {
        /// <summary>
        /// Analyzes every package in a list of given packages
        /// </summary>
        /// <param name="packages">List of WinGet package id´s</param>
        /// <param name="progress">Typical TAP progress handler pattern for progressing every step</param>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns>List of package infos (containing a package info for every analyzed package)</returns>
        Task<IEnumerable<PackageInfo>> AnalyzePackagesAsync(
            IEnumerable<string> packages,
            IProgress<PackageProgressData>? progress = default,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates every WinGet package on computer listed as 'updatable' in a given list of package infos
        /// </summary>
        /// <param name="packageInfos">List of package infos returned by AnalyzePackagesAsync() method</param>
        /// <param name="progress">Typical TAP progress handler pattern for progressing every step</param>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns>List of WinGet package id´s (containing a WinGet package id for every updated WinGet package)</returns>
        Task<IEnumerable<string>> UpdatePackagesAsync(
            IEnumerable<PackageInfo> packageInfos,
            IProgress<PackageProgressData>? progress = default,
            CancellationToken cancellationToken = default);
    }
}
