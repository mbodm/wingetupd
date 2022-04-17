namespace WinGetUpdLogging
{
    public interface ILogger
    {
        Task LogWinGetCallAsync(string call, string output, CancellationToken cancellationToken = default);
    }
}
