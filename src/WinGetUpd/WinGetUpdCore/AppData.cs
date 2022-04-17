using System.Reflection;

namespace WinGetUpdCore
{
    public static class AppData
    {
        public static string AppName => "wingetupd";
        public static string AppDate => "2022-03-26";
        public static string AppFile => $"{AppName}.exe";
        public static string PkgFile => $"packages.txt";
        public static string LogFile => $"{AppName}.log";
        public static string AppFolder => AppContext.BaseDirectory;
        public static string AppVersion => Assembly.
            GetEntryAssembly()?.
            GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.
            InformationalVersion?.
            ToString() ?? string.Empty;
    }
}
