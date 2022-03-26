using WinGetUpd;

Console.WriteLine();
Console.WriteLine($"{AppData.AppName} {AppData.AppVersion} (by MBODM {AppData.AppDate})");
Console.WriteLine();

var businessLogic = new BusinessLogic(new WinGet(new WinGetLogger()));

var errorMessage = await businessLogic.InitAsync();
if (errorMessage != string.Empty)
{
    Console.WriteLine(errorMessage);
    Environment.Exit(1);
}

var packages = await businessLogic.GetPackagesAsync();

var existsCounter = 0;
var installedCounter = 0;
var updatedCounter = 0;
var errorCounter = 0;

Console.Write($"Processing {packages.Count()} packages ...");

await businessLogic.ProcessPackagesAsync(packages, new Progress<ProgressData>(progressData =>
{
    if (progressData.Status == ProgressStatus.ErrorOccurred)
    {
        Console.Write(".");
        errorCounter++;
    }

    if (progressData.Status == ProgressStatus.PackageExists)
    {
        Console.Write(".");
        existsCounter++;

    }

    if (progressData.Status == ProgressStatus.PackageIsInstalled)
    {
        Console.Write(".");
        installedCounter++;

    }

    if (progressData.Status == ProgressStatus.PackageUpdated)
    {
        Console.Write(".");
        updatedCounter++;
    }
})).ConfigureAwait(false);

// Not 100% sure here, if the .ConfigureAwait(false) approach (when used by all async calls, in a console app) really makes
// Progress<T> using Send() for the message-queue, instead of Post(). Because in a Post() scenario it is possible for above
// progress-handler-calls to sometimes arrive not until the app already has closed (or while parts of below summary already
// have been written to console). The problem is: The amount of above progress-handler-calls is not predictable, due to the
// simple fact we not know how many packages will be updated in the process. So we can not even use some semaphore here, to
// wait until all progress-handler-calls have arrived. So instead we simply wait a short amount of time here. Since i never
// saw above problem happening in reality, this rather cheap workaround still seems better to me, than doing nothing at all.

await Task.Delay(1000);

Console.Write(" done.");

Console.WriteLine();
Console.WriteLine();

Console.WriteLine($"{existsCounter} package(s) parsed.");
Console.WriteLine($"{installedCounter} package(s) installed.");
Console.WriteLine($"{updatedCounter} package(s) updated.");

if (errorCounter != 0)
{
    Console.WriteLine($"{errorCounter} failure(s) detected. For more information have a look at the log file ('{AppData.LogFile}').");
}

Console.WriteLine();
Console.WriteLine("Have a nice day.");

Environment.Exit(0);
