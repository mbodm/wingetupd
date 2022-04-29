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

        public static void ShowNonInstalledPackagesError(IEnumerable<string> notInstalledPackages)
        {
            Console.WriteLine("Error: The package-file contains non-installed packages.");
            Console.WriteLine();
            Console.WriteLine("The following package-file entries are valid WinGet package id´s,");
            Console.WriteLine("but those packages are not already installed on this machine yet:");
            ListPackages(notInstalledPackages);
            Console.WriteLine();
            Console.WriteLine("You can use 'winget list' to show all installed packages and their package id´s.");
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

            Console.WriteLine($"{installedPackages.Count()} {PackageOrPackages(installedPackages)} installed:");
            ListPackages(installedPackages);

            Console.Write($"{updatablePackages.Count()} {PackageOrPackages(updatablePackages)} updatable");
            if (updatablePackages.Any())
            {
                Console.WriteLine(":");
                ListPackages(updatablePackages);
            }
            else
            {
                Console.WriteLine(".");
            }
        }

        public static bool AskUpdateQuestion(IEnumerable<string> updateablePackages)
        {
            Console.Write($"Update {updateablePackages.Count()} {PackageOrPackages(updateablePackages)} ? [y/N]: ");

            while (true)
            {
                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.N || key == ConsoleKey.Enter)
                {
                    Console.Write("N");
                    Console.WriteLine();
                    Console.WriteLine();

                    return false;
                }

                if (key == ConsoleKey.Y)
                {
                    Console.Write("y");
                    Console.WriteLine();
                    Console.WriteLine();

                    return true;
                }
            }
        }

        public static void ShowWinGetError(string error)
        {
            var winGetLogFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Packages\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\LocalState\DiagOutputDir");

            var winGetWebSite = "https://docs.microsoft.com/en-us/windows/package-manager/winget";

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Error: {error}");
            Console.WriteLine();
            Console.WriteLine($"For details have a look at the log file ('{BusinessLogic.AppData.LogFileName}').");
            Console.WriteLine();
            Console.WriteLine("For even more details have a look at WinGet´s own log files:");
            Console.WriteLine($"-> {winGetLogFolder}");
            Console.WriteLine();
            Console.WriteLine("You can also find further information on the WinGet site: ");
            Console.WriteLine($"-> {winGetWebSite}");
        }

        public static void ExitApp(int exitCode)
        {
            Console.WriteLine();
            Console.Write("Press any key to exit ... ");
            Console.ReadKey(true);
            Console.WriteLine();

            Environment.Exit(exitCode);
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
