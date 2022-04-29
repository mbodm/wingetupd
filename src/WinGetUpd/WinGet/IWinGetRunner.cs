namespace WinGet
{
    public interface IWinGetRunner
    {
        /// <summary>
        /// Verifies if WinGet is installed
        /// </summary>
        /// <returns>Boolean value indicating if WinGet is installed</returns>
        bool WinGetIsInstalled { get; }

        /// <summary>
        /// Starts WinGet as async process
        /// </summary>
        /// <param name="parameters">WinGet arguments/command/options (like '--version' argument, 'search' command, '--id' option, etc.)</param>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns>Model object containing how winget.exe was called as well as it´s output and it´s exit code</returns>
        Task<WinGetRunnerResult> RunWinGetAsync(string parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts WinGet as async process
        /// </summary>
        /// <param name="parameters">WinGet arguments/command/options (like '--version' argument, 'search' command, '--id' option, etc.)</param>
        /// <param name="timeout">Amount of time to wait for WinGet process, before WinGet process gets canceled. Default is 15 seconds.</param>
        /// <param name="cancellationToken">Typical TAP cancellation token pattern for task cancellation</param>
        /// <returns>Model object containing how winget.exe was called as well as it´s output and it´s exit code</returns>
        Task<WinGetRunnerResult> RunWinGetAsync(string parameters, TimeSpan timeout, CancellationToken cancellationToken = default);
    }
}
