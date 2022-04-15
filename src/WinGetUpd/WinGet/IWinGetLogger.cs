namespace WinGet
{
    public interface IWinGetLogger
    {
        Task LogAsync(string call, string output, CancellationToken cancellationToken = default);
    }
}
