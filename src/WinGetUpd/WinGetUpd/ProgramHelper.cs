using WinGetUpdCore;

namespace WinGetUpd
{
    internal static class ProgramHelper
    {
        public static void ShowUsage()
        {
            Console.WriteLine($"Usage: {AppData.AppName}.exe [--show] [--update]");
            Console.WriteLine();
            Console.WriteLine($"  --show    Show updatable packages");
            Console.WriteLine($"  --update  Update packages");
        }

        public static void ShowUnknownArgumentError(string arg)
        {
            Console.WriteLine($"Error: Unknown argument {arg}");
            Console.WriteLine();
            ShowUsage();
        }

        public static void ShowSummary(IEnumerable<PackageInfo> packageInfos)
        {
            Console.WriteLine($"{packageInfos.Count()} {GetEntryOrEntriesText(packageInfos.Count())} parsed");

            var validPackages = packageInfos.Where(packageInfo => packageInfo.IsValid).Select(packageInfo => packageInfo.Package);
            var installedPackages = packageInfos.Where(packageInfo => packageInfo.IsInstalled).Select(packageInfo => packageInfo.Package);
            var updatablePackages = packageInfos.Where(packageInfo => packageInfo.IsUpdatable).Select(packageInfo => packageInfo.Package);
            
            Console.WriteLine($"{result.ExistingPackages.Count()}/{lines} line(s) valid.");
            if (result.NonExistingPackages.Any())
            {
                Console.WriteLine($"{result.NonExistingPackages.Count()}/{lines} line(s) not valid:");
                foreach (var package in result.NonExistingPackages)
                {
                    Console.WriteLine($"  {package}");
                }
            }
            
            var valid = result.ExistingPackages.Count();
            var installed = result.InstalledPackages.Count();
            var nonInstalled = result.NonInstalledPackages.Count();

            Console.WriteLine($"{installed}/{valid} package(s) installed.");
            if (result.NonInstalledPackages.Any())
            {
                Console.WriteLine($"{nonInstalled}/{valid} package(s) not installed:");
                foreach (var package in result.NonInstalledPackages)
                {
                    Console.WriteLine($"  {package}");
                }
            }

            var updatable = result.UpdatablePackages.Count();
            Console.Write($"{updatable}/{installed} package(s) updatable");
            if (updatable < 1)
            {
                Console.WriteLine(".");
            }
            else
            {
                Console.WriteLine(":");

                foreach (var package in result.UpdatablePackages)
                {
                    Console.WriteLine($"  {package}");
                }

                Console.WriteLine($"  Use '{AppData.AppName}.exe --update' to update these packages.");
            }
        }

        public static string GetEntryOrEntriesText(int count)
        {
            return count == 1 ? "entry" : "entries";
        }

        public static string GetPackageOrPackagesText(int count)
        {
            return count == 1 ? "entry" : "entries";
        }
    }
}
