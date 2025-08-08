using System.Collections.Generic;

namespace QuazalWV
{
    public static class NotificationQueue
    {
        private static readonly object _sync = new object();
        private static List<NotificationEvent> Queue = new List<NotificationEvent>();

        public static void AddNotification(NotificationEvent n)
        {
            lock (_sync)
            {
                Queue.Add(n);
            }
        }

        public static void Update()
        {
            lock (_sync)
            {
                for (int i = 0; i < Queue.Count; i++)
                {
                    NotificationEvent n = Queue[i];
                    if (n.Timer.ElapsedMilliseconds > n.Timeout)
                    {
                        n.Send();
                        n.Timer.Stop();
                        Queue.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
    }
}
