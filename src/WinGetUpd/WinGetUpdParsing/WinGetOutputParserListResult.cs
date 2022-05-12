namespace WinGetUpdParsing
{
    public sealed record WinGetOutputParserListResult(bool IsUpdatable, string OldVersion, string NewVersion);
}
