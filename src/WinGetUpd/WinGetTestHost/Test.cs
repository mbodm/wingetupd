using System.Diagnostics;
using WinGet;

namespace WinGetTestHost
{
    internal static class Test
    {
        public static async Task RunAsync(string command, string[] packages, bool concurrentMode, bool silentMode)
        {
            var stopwatch = new Stopwatch();
            var winGetRunner = new WinGetRunner();

            stopwatch.Start();

            if (concurrentMode)
            {
                var tasks = packages.Select(async package =>
                {
                    var result = await winGetRunner.RunWinGetAsync(command, $"--exact --id {package}");

                    if (!silentMode) ShowResult(result);

                    return result;
                });

                var results = await Task.WhenAll(tasks);
            }
            else
            {
                foreach (var package in packages)
                {
                    var result = await winGetRunner.RunWinGetAsync(command, $"--exact --id {package}");

                    if (!silentMode) ShowResult(result);
                }
            }

            stopwatch.Stop();

            ShowDuration(command, stopwatch.Elapsed.Seconds);
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
