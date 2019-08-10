using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
