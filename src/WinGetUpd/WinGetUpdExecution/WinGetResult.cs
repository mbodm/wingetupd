namespace WinGetUpdExecution
{
    public sealed record WinGetResult(string ProcessCall, string ConsoleOutput, int ExitCode);
}
