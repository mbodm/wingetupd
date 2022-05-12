using System.Text.RegularExpressions;

namespace WinGetUpdParsing
{
    public sealed class WinGetOutputParser : IWinGetOutputParser
    {
        public WinGetOutputParserListResult ParseListOutput(string listOutput)
        {
            // WinGet list output example:
            /*
            Name                         ID                                    Version Verfügbar Quelle
            -------------------------------------------------------------------------------------------
            Visual Studio Community 2022 Microsoft.VisualStudio.2022.Community 17.1.6  17.2.0    winget
            */

            var regex = new Regex(@"\d+(\.\d+)+");
            var matches = regex.Matches(listOutput);

            if (!matches.Any())
            {
                var message = "Given argument is not a valid WinGet list output, since it doesn´t contain any version numbers.";
                throw new ArgumentException(message, nameof(listOutput));
            }

            if (matches.Count > 2)
            {
                var message = "Given argument is not a valid WinGet list output, since it contains more than 2 version numbers.";
                throw new ArgumentException(message, nameof(listOutput));
            }

            var oldVersion = matches.First().Value;
            var newVersion = matches.Count == 2 ? matches.Last().Value : string.Empty;
            var isUpdatable = (listOutput.Contains(" Verfügbar ") || listOutput.Contains(" Available ")) && !string.IsNullOrEmpty(newVersion);

            return new WinGetOutputParserListResult(isUpdatable, oldVersion, newVersion);
        }
    }
}
