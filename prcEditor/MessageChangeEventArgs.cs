using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prcEditor
{
    public class MessageChangeEventArgs : EventArgs
    {
        public string Message { get; }

        public MessageChangeEventArgs(string message)
        {
            Message = message;
        }
    }
}
