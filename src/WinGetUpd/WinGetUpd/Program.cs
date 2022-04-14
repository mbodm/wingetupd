using WinGetUpd;

// None of the async calls is using .ConfigureAwait(false) any longer now, since it´s useless
// in console apps. For more information about that topic, take a look at the following links:
// https://devblogs.microsoft.com/dotnet/configureawait-faq/
// https://stackoverflow.com/questions/25817703/configureawaitfalse-not-needed-in-console-win-service-apps-right

Console.WriteLine();
Console.WriteLine($"{AppData.AppName} {AppData.AppVersion} (by MBODM {AppData.AppDate})");
Console.WriteLine();

var allowedArguments = new List<string> { "--show", "--update" };

if (args.Length != 1)
{
    ProgramHelper.ShowUsage();
    Environment.Exit(0);
}

if (!allowedArguments.Contains(args[0]))
{
    ProgramHelper.ShowUnknownArgumentError(args[0]);
    Environment.Exit(1);
}

var businessLogic = new BusinessLogic(new PrerequisitesValidator(), new PackageManager(new WinGet(new WinGetLogger())));

try
{
    await businessLogic.InitAsync();
}
catch (InvalidOperationException e)
{
    Console.WriteLine($"Error: {e.Message}");
    Environment.Exit(1);
}

var packages = await businessLogic.GetPackagesAsync();

if (!packages.Any())
{
    Console.WriteLine($"Error: Package-File empty.");
    Environment.Exit(1);
}

Console.WriteLine($"Package-File found, containing {packages.Count()} line(s).");
Console.WriteLine();

Console.Write("Processing ...");

if (args[0] == "--show")
{
    BusinessLogicResult summary = null;

    try
    {
        summary = await businessLogic.GetSummaryAsync(packages, new BusinessLogicProgress(() =>
        {
            Console.Write(".");
        }));
    }
    catch (InvalidOperationException e)
    {
        Console.WriteLine(e.Message);
        Environment.Exit(1);
    }

    Console.Write(" done.");

    Console.WriteLine();
    Console.WriteLine();

    ProgramHelper.ShowSummary(summary);

    Console.WriteLine();
    Console.WriteLine("Have a nice day.");

    Environment.Exit(0);
}

// have a look at log file or winget log at:
// %LOCALAPPDATA%\Packages\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\LocalState\DiagOutputDir

Console.WriteLine();