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

if (!Tests.Installed())
{
    Console.WriteLine("WinGet is not installed.");
    Environment.Exit(1);
}

for (int i = 0; i < maxTestRepetitions; i++)
{
    await Tests.RunAsync("search", packages, concurrentMode, silentMode);
    await Tests.RunAsync("list", packages, concurrentMode, silentMode);
}

Console.WriteLine("Have a nice day.");
Environment.Exit(0);
