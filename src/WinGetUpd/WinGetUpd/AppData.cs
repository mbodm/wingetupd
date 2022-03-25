using System.Reflection;

namespace WinGetUpd
{
    internal static class AppData
    {
        public static string AppName => "wingetupd";

        public static string AppVersion
        {
            get
            {
                var version = Assembly.GetExecutingAssembly()?.GetName()?.Version;

                return version == null ? string.Empty : version.ToString();
            }
        }

        public static string AppDate => "2022-03-25";

        public static string PkgFile => $"packages.txt";

        public static string LogFile => $"{AppName}.log";
    }
}
