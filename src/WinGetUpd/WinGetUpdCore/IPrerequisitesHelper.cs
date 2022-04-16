namespace WinGetUpdCore
{
    public interface IPrerequisitesHelper
    {
        /// <summary>
        /// Verifies if package file exists
        /// </summary>
        /// <returns>Boolean value indicating if package file exists</returns>
        bool PackageFileExists();

        /// <summary>
        /// Verifies if WinGet is installed
        /// </summary>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns>Boolean value indicating if WinGet is installed</returns>
        Task<bool> WinGetExistsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies if log file can be written
        /// </summary>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns>Boolean value indicating if log file can be written</returns>
        Task<bool> CanWriteLogFileAsync(CancellationToken cancellationToken = default);
    }
}
