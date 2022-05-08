using WinGetUpdCore;

namespace WinGetUpd
{
    internal static class ProgramHelper
    {
        public static void ShowUsage(string exeFile, bool showError)
        {
            if (showError)
            {
                Console.WriteLine("Error: Unknown parameter(s).");
                Console.WriteLine();
            }

            Console.WriteLine($"Usage: {exeFile} [--no-log] [--no-confirm]");
            Console.WriteLine();
            Console.WriteLine("  --no-log      Don´t create log file (useful when running from a folder without write permissions)");
            Console.WriteLine("  --no-confirm  Don´t ask for any confirmation (useful for script integration)");
            Console.WriteLine();
            Console.WriteLine("For more information have a look at the GitHub page (https://github.com/MBODM/wingetupd)");
        }

        public static void ShowPackageFileEntries(IEnumerable<string> packageFileEntries)
        {
            Console.WriteLine($"Found package-file, containing {packageFileEntries.Count()} {EntryOrEntries(packageFileEntries)}.");
        }

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

        public static void ShowNonInstalledPackagesError(IEnumerable<string> nonInstalledPackages)
        {
            Console.WriteLine("Error: The package-file contains non-installed packages.");
            Console.WriteLine();
            Console.WriteLine("The following package-file entries are valid WinGet package id´s,");
            Console.WriteLine("but those packages are not already installed on this machine yet:");

            ListPackages(nonInstalledPackages);

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

        public static void ShowUpdatedPackages(IEnumerable<string> updatedPackages)
        {
            Console.WriteLine();
            Console.WriteLine($"{updatedPackages.Count()} {PackageOrPackages(updatedPackages)} updated:");

            ListPackages(updatedPackages);
        }

        public static void ShowGoodByeMessage()
        {
            Console.WriteLine("Have a nice day.");
        }

        public static void ShowWinGetError(string error, string log)
        {
            var winGetLogFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Packages\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\LocalState\DiagOutputDir");

            var winGetWebSite = "https://docs.microsoft.com/en-us/windows/package-manager/winget";

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Error: {error}");
            Console.WriteLine();
            Console.WriteLine($"For details have a look at the log file ('{log}').");
            Console.WriteLine();
            Console.WriteLine("For even more details have a look at WinGet´s own log files:");
            Console.WriteLine(winGetLogFolder);
            Console.WriteLine();
            Console.WriteLine("You can also find further information on the WinGet site: ");
            Console.WriteLine(winGetWebSite);
        }

        public static void ExitApp(int exitCode, bool showConfirm)
        {
            if (showConfirm)
            {
                Console.WriteLine();
                Console.Write("Press any key to exit ... ");
                Console.ReadKey(true);
                Console.WriteLine();
            }

            Environment.Exit(exitCode);
        }

        private static string EntryOrEntries<T>(IEnumerable<T> enumerable) =>
            SingularOrPlural(enumerable, "entry", "entries");

        private static string PackageOrPackages<T>(IEnumerable<T> enumerable) =>
            SingularOrPlural(enumerable, "package", "packages");

        private static string SingularOrPlural<T>(IEnumerable<T> enumerable, string singular, string plural) =>
            enumerable.Count() == 1 ? singular : plural;

        private static void ListPackages(IEnumerable<string> packages) => packages.ToList().ForEach(package =>
            Console.WriteLine($"  - {package}"));
    }
}
