namespace WinGetUpdLogging
{
    public sealed class FileLogger : IFileLogger
    {
        private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

        public FileLogger(string logFile)
        {
            LogFile = logFile ?? throw new ArgumentNullException(nameof(logFile));
        }

        public string LogFile { get; }

        public async Task<bool> CanWriteLogFileAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await File.WriteAllTextAsync(LogFile, string.Empty, cancellationToken).ConfigureAwait(false);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public async Task LogWinGetCallAsync(string call, string output, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(call))
            {
                throw new ArgumentException($"'{nameof(call)}' cannot be null or whitespace.", nameof(call));
            }

            if (output is null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            var dateTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss:fff");

            call = "\t" + call;

            output = RemoveProgressUnicodeCharsFromWinGetOuput(output);
            output = CorrectSpecificUnicodeCharsInWinGetOuput(output);
            output = AddTabsToWinGetOuput(output);

            var lines = new string[] { $"{dateTime}", call, output };

            await semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                await File.AppendAllLinesAsync(LogFile, lines, cancellationToken).ConfigureAwait(false);
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
            // This isn´t necessary when using UTF-8 Encoding for Process.StandardOutputEncoding.
            // But since we don´t control the WinGet process here, we keep this as some fallback.

            if (output.Contains("Verf├╝gbar"))
            {
                output = output.Replace("Verf├╝gbar", "Verfügbar");
            }

            return output;
        }

        private static string AddTabsToWinGetOuput(string output)
        {
            if (!string.IsNullOrWhiteSpace(output))
            {
                output = output.Replace("\n", "\n\t");
                output = output.TrimEnd('\t');
                output = output.TrimEnd('\n');

                output = "\t" + output;
            }

            return output;
        }
    }
}
