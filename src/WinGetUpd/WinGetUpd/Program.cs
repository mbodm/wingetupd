// None of the async calls is using .ConfigureAwait(false) any longer now, since it´s useless
// in console apps. For more information about that topic, take a look at the following links:
// https://devblogs.microsoft.com/dotnet/configureawait-faq/
// https://stackoverflow.com/questions/25817703/configureawaitfalse-not-needed-in-console-win-service-apps-right

using WinGetUpdLogging;
using WinGet;
using WinGetUpdCore;
using WinGetUpd;

var fileLogger = new FileLogger(BusinessLogic.AppData.LogFilePath);
var winGetRunner = new WinGetRunner();
var businessLogic = new BusinessLogic(fileLogger, winGetRunner, new WinGetManager(winGetRunner, fileLogger));

Console.WriteLine();
Console.WriteLine($"{BusinessLogic.AppData.AppName} {BusinessLogic.AppData.AppVersion} (by MBODM {BusinessLogic.AppData.AppDate})");
Console.WriteLine();

try
{
    await businessLogic.InitAsync();
    var entries = await businessLogic.GetPackageFileEntries();
    Console.WriteLine($"Found package-file, containing {entries.Count()} {ProgramHelper.EntryOrEntries(entries)}.");
    Console.WriteLine();
    Console.Write("Processing ...");
    var packageInfos = await businessLogic.AnalyzePackagesAsync(entries, new PackageProgress(_ => Console.Write(".")));
    Console.WriteLine(" finished.");
    Console.WriteLine();
    var invalidPackages = packageInfos.Where(packageInfo => !packageInfo.IsValid).Select(packageInfo => packageInfo.Package);
    if (invalidPackages.Any())
    {
        ProgramHelper.ShowInvalidPackagesError(invalidPackages);
        Environment.Exit(1);
    }
    ProgramHelper.ShowSummary(packageInfos);
    Console.WriteLine();
    ProgramHelper.ShowLogInfo();
    Console.WriteLine();
}
catch (BusinessLogicException e)
{
    Console.WriteLine($"Error: {e.Message}");
    Environment.Exit(1);
}
catch (WinGetRunnerException e)
{
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine($"Error: {e.Message}");
    Environment.Exit(1);
}
