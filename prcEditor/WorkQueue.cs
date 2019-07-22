using System;
using System.Collections.Generic;
using System.Threading;

namespace prcEditor
{
    public class WorkQueue
    {
        private Queue<EnqueuableStatus> queue { get; set; }
        private readonly object queueLock = new object();

        private Thread workerThread { get; set; }

        private string status;
        public string Status
        {
            get => status;
            set
            {
                status = value;
                RaiseMessageChangeEvent?.Invoke(this, new MessageChangeEventArgs(value));
            }
        }

        public int Count => queue.Count;

        public event EventHandler<MessageChangeEventArgs> RaiseMessageChangeEvent;

        public WorkQueue()
        {
            queue = new Queue<EnqueuableStatus>();
        }

        private void WorkerMain()
        {
            while (true)
            {
                int count;
                EnqueuableStatus next;
                lock (queueLock)
                {
                    count = queue.Count;
                    if (count > 0)
                        next = queue.Dequeue();
                    else
                    {
                        Status = null;
                        return;
                    }
                }
                if (next.NewMessage)
                    Status = next.Message;
                next.Action.Invoke();
            }
        }

        public void Enqueue(EnqueuableStatus status)
        {
            lock (queueLock)
            {
                queue.Enqueue(status);
                if (workerThread == null || !workerThread.IsAlive)
                {
                    workerThread = new Thread(WorkerMain);
                    workerThread.IsBackground = true;
                    workerThread.Start();
                }
            }
        }
    }
}
