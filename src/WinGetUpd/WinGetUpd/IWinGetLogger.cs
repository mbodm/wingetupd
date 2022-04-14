namespace WinGetUpd
{
    internal interface IWinGetLogger
    {
        void Init();
        Task LogAsync(string call, string output, CancellationToken cancellationToken = default);
    }
}
