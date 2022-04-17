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

for (int i = 0; i < maxTestRepetitions; i++)
{
    await Test.RunAsync("search", packages, concurrentMode, silentMode);
    await Test.RunAsync("list", packages, concurrentMode, silentMode);
}

Console.WriteLine("Have a nice day.");
