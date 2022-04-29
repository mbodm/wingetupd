using WinGetTestHost;

var maxTestRepetitions = 1;
var concurrentMode = false;
var silentMode = false;
var packages = new string[] {
    "Microsoft.Edge",
    "Mozilla.Firefox",
    "Microsoft.VisualStudio.2022.Community",
    "Microsoft.VisualStudioCode",
};

if (!Tests.WinGetIsInstalled())
{
    Console.WriteLine("WinGet is not installed.");
    Environment.Exit(1);
}

var timeoutReached = await Tests.TimeoutAsync(5);

if (!timeoutReached)
{
    for (int i = 0; i < maxTestRepetitions; i++)
    {
        await Tests.RunWinGetAsync("search", packages, concurrentMode, silentMode);
        await Tests.RunWinGetAsync("list", packages, concurrentMode, silentMode);
    }
}

Console.WriteLine("Have a nice day.");
Environment.Exit(0);
