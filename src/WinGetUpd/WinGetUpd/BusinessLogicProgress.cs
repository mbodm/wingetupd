namespace WinGetUpd
{
    internal sealed class BusinessLogicProgress : IProgress<object>
    {
        private readonly Action action;

        public BusinessLogicProgress(Action action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Report(object value)
        {
            action?.Invoke();
        }
    }
}
