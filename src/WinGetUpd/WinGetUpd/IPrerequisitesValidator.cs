namespace WinGetUpd
{
    internal interface IPrerequisitesValidator
    {
        bool PackageFileExists();
        Task<bool> WinGetExistsAsync(CancellationToken cancellationToken = default);
        Task<bool> CanWriteLogFileAsync(CancellationToken cancellationToken = default);
    }
}
