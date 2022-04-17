using WinGetUpdCore;

namespace WinGetUpd
{
    internal static class ProgramHelper
    {
        public static bool ShowInvalidPackagesError(IEnumerable<PackageInfo> packageInfos)
        {
            var invalidPackages = packageInfos.Where(packageInfo => !packageInfo.IsValid).Select(packageInfo => packageInfo.Package);
            if (invalidPackages.Any())
            {
                Console.WriteLine("Error: The package-file contains invalid entries.");
                Console.WriteLine();
                Console.WriteLine("The following entries are not valid WinGet package id´s:");
                invalidPackages.ToList().ForEach(package => Console.WriteLine($"  {package}"));
                Console.WriteLine();
                Console.WriteLine("You can use 'winget search' to list all valid package id´s.");
                Console.WriteLine();
                Console.WriteLine("Please verify package-file and try again.");

                return true;
            }

            return false;
        }

        public static void ShowSummary(IEnumerable<PackageInfo> packageInfos)
        {
            //var validPackages = packageInfos.Where(packageInfo => packageInfo.IsValid).Select(packageInfo => packageInfo.Package);
            //var installedPackages = packageInfos.Where(packageInfo => packageInfo.IsInstalled).Select(packageInfo => packageInfo.Package);
            //var updatablePackages = packageInfos.Where(packageInfo => packageInfo.IsUpdatable).Select(packageInfo => packageInfo.Package);

            //Console.WriteLine($"{packageInfos.Count()} {EntryOrEntries(packageInfos)} processed");
            //Console.WriteLine($"{validPackages.Count()} {PackageOrPackages(packageInfos)} found");
            //Console.WriteLine($"The following package-file entries are not valid WinGet package id´s:");
            //foreach (var package in )
            //{
            //    Console.WriteLine($"  {package}");
            //}
            //var valid = result.ExistingPackages.Count();
            //var installed = result.InstalledPackages.Count();
            //var nonInstalled = result.NonInstalledPackages.Count();

            //Console.WriteLine($"{installed}/{valid} package(s) installed.");
            //if (result.NonInstalledPackages.Any())
            //{
            //    Console.WriteLine($"{nonInstalled}/{valid} package(s) not installed:");
            //    foreach (var package in result.NonInstalledPackages)
            //    {
            //        Console.WriteLine($"  {package}");
            //    }
            //}

            //var updatable = result.UpdatablePackages.Count();
            //Console.Write($"{updatable}/{installed} package(s) updatable");
            //if (updatable < 1)
            //{
            //    Console.WriteLine(".");
            //}
            //else
            //{
            //    Console.WriteLine(":");

            //    foreach (var package in result.UpdatablePackages)
            //    {
            //        Console.WriteLine($"  {package}");
            //    }

            //    Console.WriteLine($"  Use '{AppData.AppName}.exe --update' to update these packages.");
            //}
        }

        public static string EntryOrEntries<T>(IEnumerable<T> enumerable)
        {
            return SingularOrPlural(enumerable, "entry", "entries");
        }

        public static string PackageOrPackages<T>(IEnumerable<T> enumerable)
        {
            return SingularOrPlural(enumerable, "package", "packages");
        }

        public static string IdOrIds<T>(IEnumerable<T> enumerable)
        {
            return SingularOrPlural(enumerable, "id", "id´s");
        }

        private static string SingularOrPlural<T>(IEnumerable<T> enumerable, string singular, string plural)
        {
            return enumerable.Count() == 1 ? singular : plural;
        }
    }
}
