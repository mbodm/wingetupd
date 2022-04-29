// None of the async calls is using .ConfigureAwait(false) any longer now, since it´s useless
// in console apps. For more information about that topic, take a look at the following links:
// https://devblogs.microsoft.com/dotnet/configureawait-faq/
// https://stackoverflow.com/questions/25817703/configureawaitfalse-not-needed-in-console-win-service-apps-right

using WinGet;
using WinGetUpd;
using WinGetUpdCore;
using WinGetUpdLogging;

var winGetRunner = new WinGetRunner();
var fileLogger = new FileLogger(BusinessLogic.AppData.LogFilePath);
var winGetManager = new WinGetManager(winGetRunner, fileLogger);
var businessLogic = new BusinessLogic(fileLogger, winGetRunner, winGetManager);

Console.WriteLine();
Console.WriteLine($"{BusinessLogic.AppData.AppName} v{BusinessLogic.AppData.AppVersion} (by MBODM {BusinessLogic.AppData.AppDate})");
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
        ProgramHelper.ExitApp(1);
    }

    var nonInstalledPackages = packageInfos.Where(packageInfo => !packageInfo.IsInstalled).Select(packageInfo => packageInfo.Package);
    if (nonInstalledPackages.Any())
    {
        ProgramHelper.ShowNonInstalledPackagesError(nonInstalledPackages);
        ProgramHelper.ExitApp(1);
    }

    ProgramHelper.ShowSummary(packageInfos);
    Console.WriteLine();

    var updatablePackages = packageInfos.Where(packageInfo => packageInfo.IsUpdatable).Select(packageInfo => packageInfo.Package);
    if (updatablePackages.Any())
    {
        if (ProgramHelper.AskUpdateQuestion(updatablePackages))
        {
            Console.Write("Updating ...");
            var updatedPackages = await businessLogic.UpdatePackagesAsync(packageInfos, new PackageProgress(_ => Console.Write(".")));
            Console.WriteLine(" finished.");
            Console.WriteLine();
            Console.WriteLine($"{updatedPackages.Count()} {ProgramHelper.PackageOrPackages(updatedPackages)} updated.");
        }
        else
        {
            Console.WriteLine("Canceled, no packages updated.");
        }

        Console.WriteLine();
    }

    Console.WriteLine("Have a nice day.");

    ProgramHelper.ExitApp(0);
}
catch (Exception e)
{
    switch (e)
    {
        case BusinessLogicException:
            Console.WriteLine($"Error: {e.Message}");
            break;
        case WinGetRunnerException:
            ProgramHelper.ShowWinGetError(e.Message);
            break;
        default:
            Console.WriteLine("Unexpected error occurred:");
            Console.WriteLine();
            Console.WriteLine(e.Message);
            break;
    }

    ProgramHelper.ExitApp(1);
}
