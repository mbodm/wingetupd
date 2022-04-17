namespace WinGetUpdCore
{
    public interface IPrerequisitesHelper
    {
        /// <summary>
        /// Verifies if package file exists
        /// </summary>
        /// <returns>Boolean value indicating if package file exists</returns>
        bool PackageFileExists();
    }
}
