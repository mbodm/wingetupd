namespace WinGetUpd.Config
{
    public interface IPackageFileReader
    {
        /// <summary>
        /// Path to package file.
        /// </summary>
        string PackageFile { get; }

        /// <summary>
        /// Boolean value, indicating if package file exists.
        /// </summary>
        bool PackageFileExists { get; }

        /// <summary>
        /// Asynchronously reads WinGet package id entries from package file.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the WinGet package id entries from package file.</returns>
        Task<IEnumerable<string>> ReadPackageFileAsync(CancellationToken cancellationToken = default);
    }
}
