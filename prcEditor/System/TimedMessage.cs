using System;
using System.Threading;

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
        private int OpenThreadCount = 0;
        private readonly object Locker = new object();

        private Thread TimerThread { get; set; }
        private int SleepTime { get; set; }

        public event EventHandler<TimedMsgChangedEventArgs> RaiseMessageChangeEvent;

        public TimedMessage() { }

        private void TimerMain()
        {
            Thread.Sleep(SleepTime);
            lock (Locker)
            {
                OpenThreadCount--;
                if (OpenThreadCount == 0)
                {
                    Message = null;
                }
            }
        }

        public void SetMessage(string message, int milliseconds)
        {
            SleepTime = milliseconds;
            lock (Locker)
            {
                Message = message;
                OpenThreadCount++;

                TimerThread = new Thread(TimerMain);
                TimerThread.IsBackground = true;
                TimerThread.Start();
            }
        }
    }
}