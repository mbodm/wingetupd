namespace WinGetUpd.Core
{
    public sealed class PackageProgress : IProgress<PackageProgressData>
    {
        private readonly Action<PackageProgressData> action;

        public PackageProgress(Action<PackageProgressData> action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Report(PackageProgressData value)
        {
            action?.Invoke(value);
        }
    }
}
