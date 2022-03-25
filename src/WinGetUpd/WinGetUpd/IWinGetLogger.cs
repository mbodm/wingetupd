namespace WinGetUpd
{
    internal interface IWinGetLogger
    {
        Task LogAsync(string call, string output);
    }
}
