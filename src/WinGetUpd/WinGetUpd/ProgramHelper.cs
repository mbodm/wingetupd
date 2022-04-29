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
            ListPackages(invalidPackages);
            Console.WriteLine();
            Console.WriteLine("You can use 'winget search' to list all valid package id´s.");
            Console.WriteLine();
            Console.WriteLine("Please verify package-file and try again.");
        }

        public static void ShowNotInstalledPackagesError(IEnumerable<string> notInstalledPackages)
        {
            Console.WriteLine("Error: The package-file contains non-installed packages.");
            Console.WriteLine();
            Console.WriteLine("The following package-file entries are valid WinGet package id´s,");
            Console.WriteLine("but those packages are not already installed on this machine yet:");
            ListPackages(notInstalledPackages);
            Console.WriteLine();
            Console.WriteLine("You can use 'winget list' to list all installed packages and their package id´s.");
            Console.WriteLine();
            Console.WriteLine("Please verify package-file and try again.");
        }

        public static void ShowSummary(IEnumerable<PackageInfo> packageInfos)
        {
            var validPackages = packageInfos.Where(packageInfo => packageInfo.IsValid).Select(packageInfo => packageInfo.Package);
            var installedPackages = packageInfos.Where(packageInfo => packageInfo.IsInstalled).Select(packageInfo => packageInfo.Package);
            var updatablePackages = packageInfos.Where(packageInfo => packageInfo.IsUpdatable).Select(packageInfo => packageInfo.Package);

            Console.WriteLine($"{packageInfos.Count()} package-file {EntryOrEntries(packageInfos)} processed.");
            Console.WriteLine($"{validPackages.Count()} package-file {EntryOrEntries(packageInfos)} validated.");
            Console.WriteLine($"{installedPackages.Count()} {PackageOrPackages(installedPackages)} installed on this machine:");
            ListPackages(installedPackages);
            Console.WriteLine($"{updatablePackages.Count()} {PackageOrPackages(updatablePackages)} updatable:");
            ListPackages(updatablePackages);
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

        public static string EntryOrEntries<T>(IEnumerable<T> enumerable) =>
            SingularOrPlural(enumerable, "entry", "entries");

        public static string PackageOrPackages<T>(IEnumerable<T> enumerable) =>
            SingularOrPlural(enumerable, "package", "packages");

        public static string IdOrIds<T>(IEnumerable<T> enumerable) =>
            SingularOrPlural(enumerable, "id", "id´s");

        private static string SingularOrPlural<T>(IEnumerable<T> enumerable, string singular, string plural) =>
            enumerable.Count() == 1 ? singular : plural;
        
        private static void ListPackages(IEnumerable<string> packages) => packages.ToList().ForEach(package =>
            Console.WriteLine($"  - {package}"));
    }
}
