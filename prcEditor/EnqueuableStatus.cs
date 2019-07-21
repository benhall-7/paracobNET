using System;

namespace prcEditor
{
    public class EnqueuableStatus
    {
        public Action Action { get; }
        public bool NewMessage { get; }
        public string Message { get; }

        public EnqueuableStatus(Action action)
        {
            Action = action;
            NewMessage = false;
        }
        public EnqueuableStatus(Action action, string message)
        {
            Action = action;
            NewMessage = true;
            Message = message;
        }
    }
}
