namespace WinGet
{
    public interface IWinGetRunner
    {
        Task<WinGetRunnerResult> RunWinGetAsync(string command, string options, CancellationToken cancellationToken = default);
    }
}
