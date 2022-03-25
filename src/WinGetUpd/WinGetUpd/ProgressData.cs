namespace WinGetUpd
{
    internal sealed class ProgressData
    {
        public string Package { get; set; } = string.Empty;
        public ProgressStatus Status { get; set; }
        public string Error { get; set; } = string.Empty;
    }
}
