namespace WinGetUpdLogging
{
    public interface IFileLogger
    {
        /// <summary>
        /// Path to log file.
        /// </summary>
        string LogFile { get; }

        /// <summary>
        /// Asynchronously verifies that log file can be written.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents a boolean value, indicating if log file can be written.</returns>
        Task<bool> CanWriteLogFileAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously writes a WinGet call to log file.
        /// </summary>
        /// <param name="call">The WinGet call with parameters.</param>
        /// <param name="output">The WinGet call console output.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task.</returns>
        Task LogWinGetCallAsync(string call, string output, CancellationToken cancellationToken = default);
    }
}
