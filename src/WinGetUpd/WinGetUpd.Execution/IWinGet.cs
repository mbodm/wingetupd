namespace WinGetUpd.Execution
{
    public interface IWinGet
    {
        /// <summary>
        /// Verifies if WinGet application is installed.
        /// </summary>
        /// <returns>A boolean value, indicating if WinGet is installed on this machine.</returns>
        bool WinGetIsInstalled { get; }

        /// <summary>
        /// Asynchronously runs WinGet application.
        /// </summary>
        /// <param name="parameters">The WinGet arguments (like '--version'), WinGet commands (like 'search') and WinGet options (like '--id').</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents an object, containing data about how WinGet was called, as well as it´s output and it´s exit code.</returns>
        Task<WinGetResult> RunWinGetAsync(string parameters, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        /// <param name="timeout">The amount of time to wait for WinGet process, before process gets canceled. Default implementation is 30 seconds.</param>
        Task<WinGetResult> RunWinGetAsync(string parameters, TimeSpan timeout, CancellationToken cancellationToken = default);
    }
}
