using System.Reflection;

namespace PackageManager
{
    internal static class AppData
    {
        public static string AppName => "wingetupd";

        public static string AppVersion
        {
            get
            {
                var version = Assembly.GetExecutingAssembly()?.GetName()?.Version;

                return version == null ? string.Empty : version.ToString(3);
            }
        }

        public static string AppDate => "2022-03-26";

        public static string PkgFile => $"packages.txt";

        public static string LogFile => $"{AppName}.log";
    }
}
