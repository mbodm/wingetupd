using System.ComponentModel;
using System.Diagnostics;

namespace WinGetUpd
{
    internal sealed class PrerequisitesValidator : IPrerequisitesValidator
    {
        public bool PackageFileExists()
        {
            return File.Exists($"{AppData.PkgFile}");
        }

        public async Task<bool> WinGetExistsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "winget.exe",
                    Arguments = "--version",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                });

                if (process != null)
                {
                    await process.WaitForExitAsync(cancellationToken);

                    return true;
                }
            }
            catch (Win32Exception)
            {
                // Suppress exception
            }

            return false;
        }

        public async Task<bool> CanWriteLogFileAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await File.WriteAllTextAsync(AppData.LogFile, string.Empty, cancellationToken);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
