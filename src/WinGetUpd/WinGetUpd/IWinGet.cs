namespace WinGetUpd
{
    internal interface IWinGet
    {
        Task<bool> PackageExistsAsync(string id);
        Task<bool> PackageIsInstalledAsync(string id);
        Task<bool> PackageHasUpdateAsync(string id);
        Task UpdatePackageAsync(string id);
    }
}
