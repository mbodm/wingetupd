namespace WinGetUpdPackages
{
    public interface IPackageManager
    {
        /// <summary>
        /// Boolean value, indicating if this module will log WinGet calls.
        /// </summary>
        bool LogWinGetCalls { get; set; }

        /// <summary>
        /// Asynchronously verifies existence of a WinGet package, by running WinGet 'search' command.
        /// </summary>
        /// <param name="package">Some WinGet package id.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents a boolean value, indicating if given package is a valid WinGet package id.</returns>
        Task<bool> SearchPackageAsync(string package, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously determines installation and update state of a WinGet package, by running WinGet 'list' command.
        /// </summary>
        /// <param name="package">Some WinGet package id.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents an object, containing installation and update state of a WinGet package.</returns>
        Task<PackageManagerListResult> ListPackageAsync(string package, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously updates a WinGet package, by running WinGet 'upgrade' command.
        /// </summary>
        /// <param name="package">Some WinGet package id.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents a boolean value, indicating if WinGet package has been successfully updated.</returns>
        Task<bool> UpgradePackageAsync(string package, CancellationToken cancellationToken = default);
    }
}
