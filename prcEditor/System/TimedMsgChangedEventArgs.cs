using System;

namespace prcEditor
{
    public class TimedMsgChangedEventArgs : EventArgs
    {
        public string Message { get; }

        public TimedMsgChangedEventArgs(string message)
        {
            Message = message;
        }
    }
}
