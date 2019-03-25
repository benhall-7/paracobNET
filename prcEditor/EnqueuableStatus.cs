using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
