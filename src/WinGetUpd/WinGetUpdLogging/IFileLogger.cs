namespace WinGetUpdLogging
{
    public interface IFileLogger
    {
        /// <summary>
        /// Verifies if log file can be written
        /// </summary>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns>Boolean value indicating if log file can be written</returns>
        Task<bool> CanWriteLogFileAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        /// <param name="output"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task LogWinGetCallAsync(string call, string output, CancellationToken cancellationToken = default);
    }
}
