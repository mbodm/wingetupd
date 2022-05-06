namespace WinGetUpdCore
{
    public sealed class PackageFileReader : IPackageFileReader
    {
        private readonly string packageFile;

        public PackageFileReader(string packageFile)
        {
            this.packageFile = packageFile ?? throw new ArgumentNullException(nameof(packageFile));
        }

        public string PackageFile => packageFile;

        public bool PackageFileExists => File.Exists(packageFile);

        public async Task<IEnumerable<string>> ReadPackageFileAsync(CancellationToken cancellationToken = default)
        {
            var lines = await File.ReadAllLinesAsync(packageFile, cancellationToken).ConfigureAwait(false);

            var nonWhiteSpaceEntries = lines.Where(line => !string.IsNullOrWhiteSpace(line));

            return nonWhiteSpaceEntries;
        }
    }
}
