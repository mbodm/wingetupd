using System.Runtime.Serialization;

namespace WinGet
{
    [Serializable]
    public class WinGetRunnerException : Exception
    {
        public WinGetRunnerException() { }
        public WinGetRunnerException(string message) : base(message) { }
        public WinGetRunnerException(string message, Exception inner) : base(message, inner) { }
        protected WinGetRunnerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
