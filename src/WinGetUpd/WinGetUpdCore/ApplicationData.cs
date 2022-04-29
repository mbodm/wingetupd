using System.Reflection;

namespace WinGetUpdCore
{
    public sealed record ApplicationData
    {
        public string AppName => "wingetupd";
        public string AppDate => "2022-04-29";
        public string AppVersion => Assembly.
            GetEntryAssembly()?.
            GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.
            InformationalVersion?.
            ToString() ?? "Unknown";
        public string AppFolder => AppContext.BaseDirectory;
        public string AppFileName => $"{AppName}.exe";
        public string AppFilePath => Path.Combine(AppFolder, AppFileName);
        public string PkgFileName => "packages.txt";
        public string PkgFilePath => Path.Combine(AppFolder, PkgFileName);
        public string LogFileName => $"{AppName}.log";
        public string LogFilePath => Path.Combine(AppFolder, LogFileName);
    }
}
