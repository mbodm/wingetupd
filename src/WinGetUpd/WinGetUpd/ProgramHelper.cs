using WinGetUpdCore;

namespace WinGetUpd
{
    internal static class ProgramHelper
    {
        public static void ShowInvalidPackagesError(IEnumerable<string> invalidPackages)
        {
            Console.WriteLine("Error: The package-file contains invalid entries.");
            Console.WriteLine();
            Console.WriteLine("The following package-file entries are not valid WinGet package id´s:");
            invalidPackages.ToList().ForEach(package => Console.WriteLine($"  {package}"));
            Console.WriteLine();
            Console.WriteLine("You can use 'winget search' to list all valid package id´s.");
            Console.WriteLine();
            Console.WriteLine("Please verify package-file and try again.");
        }

        public static void ShowSummary(IEnumerable<PackageInfo> packageInfos)
        {
            var validPackages = packageInfos.Where(packageInfo => packageInfo.IsValid).Select(packageInfo => packageInfo.Package);
            var installedPackages = packageInfos.Where(packageInfo => packageInfo.IsInstalled).Select(packageInfo => packageInfo.Package);
            var updatablePackages = packageInfos.Where(packageInfo => packageInfo.IsUpdatable).Select(packageInfo => packageInfo.Package);

            Console.WriteLine($"{packageInfos.Count()} package-file {EntryOrEntries(packageInfos)} processed.");
            Console.WriteLine($"{validPackages.Count()} package-file {EntryOrEntries(packageInfos)} verified (valid WinGet package id).");
            
            Console.WriteLine($"{installedPackages.Count()} {PackageOrPackages(installedPackages)} installed on this machine:");
            foreach (var package in installedPackages)
            {
                Console.WriteLine($"  {package}");
            }
            
            Console.WriteLine($"{updatablePackages.Count()} {PackageOrPackages(updatablePackages)} updatable:");
            foreach (var package in updatablePackages)
            {
                Console.WriteLine($"  {package}");
            }
        }

        public static void ShowLogInfo()
        {
            Console.WriteLine($"For details have a look at the log file ('{BusinessLogic.AppData.LogFileName}').");
            Console.WriteLine();
            Console.WriteLine($"For even more details have a look at WinGet´s log file here:");
            var winGetLogFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Packages\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\LocalState\DiagOutputDir");
            Console.WriteLine(winGetLogFolder);
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
