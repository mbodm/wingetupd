namespace PackageManager
{
    public sealed record PackageInfo(string Package, bool IsValid, bool IsInstalled, bool IsUpdatable);
}
