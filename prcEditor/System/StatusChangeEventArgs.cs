using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
