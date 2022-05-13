// None of the async calls is using .ConfigureAwait(false) any longer now, since it´s useless
// in console apps. For more information about that topic, take a look at the following links:
// https://devblogs.microsoft.com/dotnet/configureawait-faq/
// https://stackoverflow.com/questions/25817703/configureawaitfalse-not-needed-in-console-win-service-apps-right

using WinGetUpd;
using WinGetUpd.Config;
using WinGetUpd.Core;
using WinGetUpd.Execution;
using WinGetUpd.Logging;
using WinGetUpd.Packages;
using WinGetUpd.Parsing;

Console.WriteLine();
Console.WriteLine($"{ProgramData.AppName} v{ProgramData.AppVersion} (by MBODM {ProgramData.AppDate})");
Console.WriteLine();

ProgramParams.Args = args;

if (!ProgramParams.ArgsValid)
{
    ProgramHelper.ShowUsage(ProgramData.AppFileName, !ProgramParams.ShowHelp);
    Environment.Exit(1);
}

try
{
    var fileLogger = new FileLogger(ProgramData.LogFilePath);
    var packageFileReader = new PackageFileReader(ProgramData.PkgFilePath);
    var winGet = new WinGet();
    var winGetOutputParser = new WinGetOutputParser();
    var packageManager = new PackageManager(winGet, fileLogger, winGetOutputParser);
    var businessLogic = new BusinessLogic(winGet, fileLogger, packageManager, packageFileReader);

    await businessLogic.InitAsync(!ProgramParams.NoLog);

    var entries = await businessLogic.GetPackageFileEntriesAsync();
    ProgramHelper.ShowPackageFileEntries(entries);
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

    var nonInstalledPackages = packageInfos.Where(packageInfo => !packageInfo.IsInstalled).Select(packageInfo => packageInfo.Package);
    if (nonInstalledPackages.Any())
    {
        ProgramHelper.ShowNonInstalledPackagesError(nonInstalledPackages);
        Environment.Exit(1);
    }

    ProgramHelper.ShowSummary(packageInfos);
    Console.WriteLine();

    var updatablePackages = packageInfos.Where(packageInfo => packageInfo.IsUpdatable).Select(packageInfo => packageInfo.Package);
    if (updatablePackages.Any())
    {
        // This boolean statement is written a bit extraordinary to make the logic a bit more readable.

        if ((ProgramParams.NoConfirm == false) && (ProgramHelper.AskUpdateQuestion(updatablePackages) == false))
        {
            Console.WriteLine("Canceled, no packages updated.");
        }
        else
        {
            Console.Write("Updating ......");
            var updatedPackages = updatablePackages;  // await businessLogic.UpdatePackagesAsync(packageInfos, new PackageProgress(_ => Console.Write("...")));
            Console.WriteLine(" finished.");
            ProgramHelper.ShowUpdatedPackages(updatedPackages);
        }

        Console.WriteLine();
    }

    ProgramHelper.ShowGoodByeMessage();
    Environment.Exit(0);
}
catch (Exception e)
{
    switch (e)
    {
        case BusinessLogicException:
            Console.WriteLine($"Error: {e.Message}");
            break;
        case WinGetException:
            ProgramHelper.ShowWinGetError(e.Message, ProgramData.LogFileName);
            break;
        default:
            Console.WriteLine("Unexpected error occurred:");
            Console.WriteLine();
            Console.WriteLine(e.Message);
            break;
    }

    Environment.Exit(1);
}
