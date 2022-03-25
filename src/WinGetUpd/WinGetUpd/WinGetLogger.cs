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
            if (output.Contains("Name ") && output.Contains("---"))
            {
                return output[output.IndexOf("Name ")..];
            }

            if (output.Contains("Fehler ") && output.Contains("---"))
            {
                return output[output.IndexOf("Fehler ")..];
            }

            if (output.Contains("Es wurde kein installiertes Paket gefunden"))
            {
                return output[output.IndexOf("Es wurde kein installiertes Paket gefunden")..];
            }

            return output;
        }

        private static string AddTabsToWinGetOuput(string output)
        {
            output = output.Replace("\r\n", "\r\n\t");
            output = output.TrimEnd('\t');
            output = output.TrimEnd('\n');
            output = output.TrimEnd('\r');
            output = "\t" + output;

            return output;
        }
    }
}
