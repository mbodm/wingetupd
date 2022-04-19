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
    Console.WriteLine(" Finished.");
    Console.WriteLine();
    if (ProgramHelper.ShowInvalidPackagesError(packageInfos))
    {
        Environment.Exit(1);
    }
    ProgramHelper.ShowSummary(packageInfos);
}
catch (BusinessLogicException e)
{
    Console.WriteLine($"Error: {e.Message}");
    Environment.Exit(1);
}

// have a look at log file or winget log at:
// %LOCALAPPDATA%\Packages\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\LocalState\DiagOutputDir

Console.WriteLine();