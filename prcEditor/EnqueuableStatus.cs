using System;

namespace prcEditor
{
    public class EnqueuableStatus
    {
        public Action Action { get; set; }
        public string Message { get; set; }

        public EnqueuableStatus(Action action, string message)
        {
            Action = action;
            Message = message;
        }
    }
}
