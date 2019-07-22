using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace prcEditor
{
    public class TimedMessage
    {
        private string message { get; set; }
        private string Message
        {
            get => message;
            set
            {
                message = value;
                RaiseMessageChangeEvent?.Invoke(this, new TimedMsgChangedEventArgs(value));
            }
        }
        private readonly object messageLock = new object();

        private Thread timerThread { get; set; }
        private int sleepTime { get; set; }

        public event EventHandler<TimedMsgChangedEventArgs> RaiseMessageChangeEvent;

        public TimedMessage() { }

        private void TimerMain()
        {
            Thread.Sleep(sleepTime);
            lock (messageLock)
            {
                Message = null;
            }
        }

        public void SetMessage(string message, int milliseconds)
        {
            sleepTime = milliseconds;
            lock (messageLock)
            {
                Message = message;

                //TODO: thread isn't guaranteed to be sleeping
                if (timerThread?.IsAlive == true)
                    timerThread.Interrupt();
                timerThread = new Thread(TimerMain);
                timerThread.IsBackground = true;
                timerThread.Start();
            }
        }
    }
}
