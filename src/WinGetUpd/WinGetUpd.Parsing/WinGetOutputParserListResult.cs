namespace WinGetUpd.Parsing
{
    public sealed record WinGetOutputParserListResult(bool IsUpdatable, string OldVersion, string NewVersion);
}
