namespace PackageManager
{
    public sealed record WinGetWrapperListResult(string Package, bool IsInstalled, bool IsUpdatable);
}
