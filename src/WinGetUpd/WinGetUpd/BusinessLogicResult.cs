namespace PackageManager
{
    internal sealed record BusinessLogicResult(
        IEnumerable<string> AllPackages,
        IEnumerable<string> ExistingPackages,
        IEnumerable<string> NonExistingPackages,
        IEnumerable<string> InstalledPackages,
        IEnumerable<string> NonInstalledPackages,
        IEnumerable<string> UpdatablePackages,
        IEnumerable<string> NonUpdatablePackages,
        IEnumerable<string> UpdatedPackages,
        IEnumerable<string> NonUpdatedPackages);
}
