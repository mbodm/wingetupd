namespace WinGetUpdCore
{
    public interface IWinGetManager
    {
        /// <summary>
        /// Boolean value indicating if this module will log WinGet calls
        /// </summary>
        bool LogWinGetCalls { get; set; }

        /// <summary>
        /// Validates existence of WinGet package by running 'search' command
        /// </summary>
        /// <param name="package">WinGet package id</param>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns>Boolean value indicating if given package is a valid WinGet package id and therefore exists</returns>
        Task<bool> SearchPackageAsync(string package, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns installation and update state of WinGet package by running 'list' command
        /// </summary>
        /// <param name="package">WinGet package id</param>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns>Model object containing installation and update state of WinGet package</returns>
        Task<WinGetManagerListResult> ListPackageAsync(string package, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates WinGet package by running 'upgrade' command
        /// </summary>
        /// <param name="package">WinGet package id</param>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns>Boolean value indicating if WinGet package has been successfully updated</returns>
        Task<bool> UpgradePackageAsync(string package, CancellationToken cancellationToken = default);
    }
}
