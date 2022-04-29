using System.Diagnostics;
using WinGet;

namespace WinGetTestHost
{
    internal static class Tests
    {
        public static bool WinGetIsInstalled()
        {
            var winGetRunner = new WinGetRunner();

            return winGetRunner.WinGetIsInstalled;
        }

        public static async Task RunWinGetAsync(string command, string[] packages, bool concurrentMode, bool silentMode)
        {
            var stopwatch = new Stopwatch();
            var winGetRunner = new WinGetRunner();

            stopwatch.Start();

            if (concurrentMode)
            {
                var tasks = packages.Select(async package =>
                {
                    var result = await winGetRunner.RunWinGetAsync($"{command} --exact --id {package}");

                    if (!silentMode) ShowResult(result);

                    return result;
                });

                await Task.WhenAll(tasks);
            }
            else
            {
                foreach (var package in packages)
                {
                    var result = await winGetRunner.RunWinGetAsync($"{command} --exact --id {package}");

                    if (!silentMode) ShowResult(result);
                }
            }

            stopwatch.Stop();

            ShowDuration(command, stopwatch.Elapsed.Seconds);
        }

        public static async Task<bool> TimeoutAsync(uint seconds)
        {
            // Play around with WinGetRunner timeout for this testing

            var winGetRunner = new WinGetRunner();

            try
            {
                var result = await winGetRunner.RunWinGetAsync("search", TimeSpan.FromSeconds(seconds));

                ShowResult(result);

                return false;
            }
            catch (WinGetRunnerException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();

                return true;
            }
        }

        private static void ShowResult(WinGetRunnerResult result)
        {
            Console.WriteLine(result.ProcessCall);
            Console.WriteLine(result.ConsoleOutput);
            Console.WriteLine();
        }

        private static void ShowDuration(string command, int seconds)
        {
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine($"{command.ToUpper()} finished after {seconds} seconds.");
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
