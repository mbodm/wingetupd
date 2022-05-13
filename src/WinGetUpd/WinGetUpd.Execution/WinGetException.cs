using System.Runtime.Serialization;

namespace WinGetUpd.Execution
{
    [Serializable]
    public class WinGetException : Exception
    {
        public WinGetException() { }
        public WinGetException(string message) : base(message) { }
        public WinGetException(string message, Exception inner) : base(message, inner) { }
        protected WinGetException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
