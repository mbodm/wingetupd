namespace WinGetUpd
{
    internal interface IWinGet
    {
        Task<WinGetResult> SearchAsync(string id, CancellationToken cancellationToken = default);
        Task<WinGetResult> ListAsync(string id, CancellationToken cancellationToken = default);
        Task<WinGetResult> UpgradeAsync(string id, CancellationToken cancellationToken = default);
    }
}
