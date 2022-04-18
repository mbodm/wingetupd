namespace WinGetUpdLogging
{
    public interface IFileLogger
    {
        /// <summary>
        /// Path to log file
        /// </summary>
        string LogFile { get; }

        /// <summary>
        /// Verifies that log file can be written
        /// </summary>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns>Boolean value indicating if log file can be written</returns>
        Task<bool> CanWriteLogFileAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes WinGet call to log file
        /// </summary>
        /// <param name="call">The WinGet call with parameters</param>
        /// <param name="output">The WinGet call console output</param>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns></returns>
        Task LogWinGetCallAsync(string call, string output, CancellationToken cancellationToken = default);
    }
}
