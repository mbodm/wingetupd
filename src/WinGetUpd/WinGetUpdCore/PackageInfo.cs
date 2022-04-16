namespace WinGetUpdCore
{
    public sealed record PackageInfo(string Package, bool IsValid, bool IsInstalled, bool IsUpdatable);
}
