namespace WinGetUpdPackageManagement
{
    public sealed record PackageManagerListResult(string Package, bool IsInstalled, bool IsUpdatable);
}
