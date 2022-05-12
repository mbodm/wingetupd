using System.Reflection;

namespace WinGetUpd
{
    internal static class ProgramData
    {
        public static string AppName => "wingetupd";
        public static string AppDate => "2022-05-12";
        public static string AppVersion => Assembly.
            GetEntryAssembly()?.
            GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.
            InformationalVersion?.
            ToString() ?? "0.0.0-unknown";
        public static string AppFolder => AppContext.BaseDirectory;
        public static string AppFileName => $"{AppName}.exe";
        public static string AppFilePath => Path.Combine(AppFolder, AppFileName);
        public static string PkgFileName => "packages.txt";
        public static string PkgFilePath => Path.Combine(AppFolder, PkgFileName);
        public static string LogFileName => $"{AppName}.log";
        public static string LogFilePath => Path.Combine(AppFolder, LogFileName);
    }
}
