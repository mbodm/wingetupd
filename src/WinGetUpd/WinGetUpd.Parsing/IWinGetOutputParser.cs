namespace WinGetUpd.Parsing
{
    public interface IWinGetOutputParser
    {
        WinGetOutputParserListResult ParseListOutput(string listOutput);
    }
}
