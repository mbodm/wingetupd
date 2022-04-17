using System.ComponentModel;
using System.Diagnostics;

namespace WinGetUpdCore
{
    public sealed class PrerequisitesHelper : IPrerequisitesHelper
    {
        public bool PackageFileExists()
        {
            return File.Exists($"{AppData.PkgFile}");
        }

        public async Task<bool> CanWriteLogFileAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await File.WriteAllTextAsync(AppData.LogFile, string.Empty, cancellationToken).ConfigureAwait(false);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
