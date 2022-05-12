namespace WinGetUpdParsing
{
    public interface IWinGetOutputParser
    {
        WinGetOutputParserListResult ParseListOutput(string listOutput);
    }
}
