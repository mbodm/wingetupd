namespace WinGetUpd
{
    internal sealed class WinGetLogger : IWinGetLogger
    {
        private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

        private bool isInitialized;

        public void Init()
        {
            if (!isInitialized)
            {
                DeleteLogFile();

                isInitialized = true;
            }
        }

        public async Task LogAsync(string call, string output, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(call))
            {
                throw new ArgumentException($"'{nameof(call)}' cannot be null or whitespace.", nameof(call));
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                throw new ArgumentException($"'{nameof(output)}' cannot be null or whitespace.", nameof(output));
            }

            await semaphoreSlim.WaitAsync(cancellationToken);

            try
            {
                var dateTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss:fff");

                call = "\t" + call;

                output = RemoveProgressUnicodeCharsFromWinGetOuput(output);
                output = CorrectSpecificUnicodeCharsInWinGetOuput(output);
                output = AddTabsToWinGetOuput(output);

                var lines = new string[] { $"{dateTime}", call, output };

                await File.AppendAllLinesAsync(AppData.LogFile, lines, cancellationToken);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private static void DeleteLogFile()
        {
            if (File.Exists(AppData.LogFile))
            {
                File.Delete(AppData.LogFile);
            }
        }

        private static string RemoveProgressUnicodeCharsFromWinGetOuput(string output)
        {
            if (output.Contains('\b'))
            {
                output = output.Replace("\b|", string.Empty);
                output = output.Replace("\b/", string.Empty);
                output = output.Replace("\b-", string.Empty);
                output = output.Replace("\b\\", string.Empty);
                output = output.Replace("\b", string.Empty);
                output = output.Replace("\r", string.Empty);

                output = output.Trim();
            }

            return output;
        }

        private static string CorrectSpecificUnicodeCharsInWinGetOuput(string output)
        {
            if (output.Contains("Verf├╝gbar"))
            {
                output = output.Replace("Verf├╝gbar", "Verfügbar");
            }

            return output;
        }

        private static string AddTabsToWinGetOuput(string output)
        {
            output = output.Replace("\n", "\n\t");
            output = output.TrimEnd('\t');
            output = output.TrimEnd('\n');

            output = "\t" + output;

            return output;
        }
    }
}
