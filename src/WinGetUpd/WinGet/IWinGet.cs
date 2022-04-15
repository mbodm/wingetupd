namespace WinGet
{
    public interface IWinGet
    {
        Task<WinGetResult> RunWinGetAsync(string command, string options, CancellationToken cancellationToken = default);
    }
}
