namespace WinGetUpd
{
    internal sealed class WinGetLogger : IWinGetLogger
    {
        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);

        public WinGetLogger()
        {
            if (File.Exists(AppData.LogFile))
            {
                File.Delete(AppData.LogFile);
            }
        }

        public async Task LogAsync(string call, string output)
        {
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);

            try
            {
                var dateTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

                call = "\t" + call;

                output = RemoveProgressUnicodeCharsFromWinGetOuput(output);
                output = CorrectSpecificUnicodeCharsInWinGetOuput(output);
                output = AddTabsToWinGetOuput(output);

                var lines = new string[] { $"[{dateTime}]", call, output };

                await File.AppendAllLinesAsync(AppData.LogFile, lines).ConfigureAwait(false);
            }
            finally
            {
                semaphoreSlim.Release();
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
