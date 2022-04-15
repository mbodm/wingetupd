namespace PackageManager
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

        public static void ShowSummary(BusinessLogicResult result)
        {
            var parsed = result.ExistingPackages.Count() + result.NonExistingPackages.Count();
            var lines = result.AllPackages.Count();
            

            Console.WriteLine($"{parsed}/{lines} line(s) parsed.");
            
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
    }
}
