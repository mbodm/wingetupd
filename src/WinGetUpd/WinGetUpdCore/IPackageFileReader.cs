namespace WinGetUpdCore
{
    public interface IPackageFileReader
    {
        /// <summary>
        /// Path to package file
        /// </summary>
        string PackageFile { get; }

        /// <summary>
        /// Boolean value indicating if package file exists
        /// </summary>
        bool PackageFileExists { get; }

        /// <summary>
        /// Read WinGet package id entries from package file
        /// </summary>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns>List of WinGet package id entries from package file</returns>
        Task<IEnumerable<string>> ReadPackageFileAsync(CancellationToken cancellationToken = default);
    }
}
