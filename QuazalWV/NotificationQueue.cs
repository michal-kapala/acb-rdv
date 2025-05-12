using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuazalWV
{
    public static class NotificationQueue
    {
        private static readonly object _sync = new object();
        private static List<NotificationQueueEntry> queue = new List<NotificationQueueEntry>();

        public static void AddNotification(NotificationQueueEntry n)
        {
            lock (_sync)
            {
                queue.Add(n);
            }
        }

        public static void Update()
        {
            lock (_sync)
            {
                for (int i = 0; i < queue.Count; i++)
                {
                    NotificationQueueEntry n = queue[i];
                    if (n.timer.ElapsedMilliseconds > n.timeout)
                    {
                        n.Execute();
                        n.timer.Stop();
                        queue.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
    }
}
