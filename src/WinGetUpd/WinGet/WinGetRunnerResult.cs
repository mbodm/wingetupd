namespace WinGet
{
    public sealed record WinGetRunnerResult(string ProcessCall, string ConsoleOutput, int ExitCode);
}
