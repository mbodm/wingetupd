namespace WinGetUpd.Config
{
    public sealed class PackageFileReader : IPackageFileReader
    {
        public PackageFileReader(string packageFile)
        {
            PackageFile = packageFile ?? throw new ArgumentNullException(nameof(packageFile));
        }

        public string PackageFile { get; }

        public bool PackageFileExists => File.Exists(PackageFile);

        public async Task<IEnumerable<string>> ReadPackageFileAsync(CancellationToken cancellationToken = default)
        {
            var lines = await File.ReadAllLinesAsync(PackageFile, cancellationToken).ConfigureAwait(false);

            var nonWhiteSpaceEntries = lines.Where(line => !string.IsNullOrWhiteSpace(line));

            return nonWhiteSpaceEntries;
        }
    }
}
