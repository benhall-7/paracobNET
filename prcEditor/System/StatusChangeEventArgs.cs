using System;

namespace prcEditor
{
    public class StatusChangeEventArgs : EventArgs
    {
        public string Message { get; }

        public StatusChangeEventArgs(string message)
        {
            Message = message;
        }
    }
}
