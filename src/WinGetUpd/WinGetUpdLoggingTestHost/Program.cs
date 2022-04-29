using WinGetUpdLogging;

var fileLogger = new FileLogger("Test.log");

Console.WriteLine("Log file:");
Console.WriteLine(Path.GetFullPath(fileLogger.LogFile));
Console.WriteLine();

Console.WriteLine("Log file writable:");
if (await fileLogger.CanWriteLogFileAsync())
{
    Console.WriteLine("Yes");
    Console.WriteLine();
}
else
{
    Console.WriteLine("No");
    Console.WriteLine();
    Console.WriteLine("Error: Can not write log file. Program canceled.");
    Environment.Exit(1);
}

Console.WriteLine("Write log entries concurrent ...");
var tasks = new List<Task>
{
    fileLogger.LogWinGetCallAsync("winget.exe --version1", "v1.0.0"),
    fileLogger.LogWinGetCallAsync("winget.exe --version2", "v2.0.0"),
    fileLogger.LogWinGetCallAsync("winget.exe --version3", "v3.0.0"),
};
await Task.WhenAll(tasks);
Console.WriteLine($"{tasks.Count} log entries successfully written.");
Console.WriteLine();

Console.WriteLine("Written log entries:");
var lines = await File.ReadAllLinesAsync(fileLogger.LogFile);
lines.ToList().ForEach(line => Console.WriteLine(line));
Console.WriteLine();

Console.WriteLine("Have a nice day.");
Environment.Exit(0);
