namespace WinGetUpd
{
    internal static class ProgramParams
    {
        public static IEnumerable<string> SupportedArgs { get; } = new string[] { "--no-log", "--no-confirm" };
        public static IEnumerable<string> Args { get; set; } = Enumerable.Empty<string>();
        public static bool ArgsValid => Args.All(arg => SupportedArgs.Contains(arg));
        public static bool NoLog => Args.Contains("--no-log");
        public static bool NoConfirm => Args.Contains("--no-confirm");
        public static bool ShowHelp => Args.Count() == 1 && Args.First() == "--help";
    }
}
