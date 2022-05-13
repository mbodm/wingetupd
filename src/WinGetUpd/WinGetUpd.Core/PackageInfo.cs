namespace WinGetUpd.Core
{
    public sealed record PackageInfo(string Package, bool IsValid, bool IsInstalled, bool IsUpdatable, string InstalledVersion, string UpdateVersion);
}
