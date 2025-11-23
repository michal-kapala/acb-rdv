using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;

namespace QuazalWV
{
    public class ExpiringLockManager<TKey>
    {
        private class LockWrapper
        {
            public object LockObject { get; } = new object();
            public DateTime LastUsed { get; set; } = DateTime.UtcNow;
        }

        private readonly ConcurrentDictionary<TKey, LockWrapper> _locks = new();
        private readonly TimeSpan _expiration;
        private readonly Timer _cleanupTimer;

        public ExpiringLockManager(TimeSpan expiration, TimeSpan cleanupInterval)
        {
            _expiration = expiration;
            _cleanupTimer = new Timer(Cleanup, null, cleanupInterval, cleanupInterval);
        }

        public object GetLock(TKey key)
        {
            var wrapper = _locks.GetOrAdd(key, _ => new LockWrapper());
            wrapper.LastUsed = DateTime.UtcNow;
            return wrapper.LockObject;
        }

        private void Cleanup(object state)
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in _locks)
            {
                if (now - kvp.Value.LastUsed > _expiration)
                {
                    _locks.TryRemove(kvp.Key, out _);
                    var client = Global.Clients.Find(c => c.ep.Address.ToString() == kvp.Key.ToString());
                    if (client != null)
                    {
                        Log.WriteLine(1, $"TIMEOUT", LogSource.PRUDP, Color.Gray, client);
                        var rels = DbHelper.GetRelationships(client.User.Pid, (byte)PlayerRelationship.Friend);
                        uint friendPid;
                        ClientInfo friend;
                        foreach (var relationship in rels)
                        {
                            friendPid = relationship.RequesterPid == client.User.Pid ? relationship.RequesteePid : relationship.RequesterPid;
                            friend = Global.Clients.Find(c => c.User.Pid == friendPid);
                            if (friend != null)
                                NotificationManager.FriendStatusChanged(friend, client.User.Pid, client.User.Name, false);
                        }
                        Global.Clients.Remove(client);
                    }
                }
            }
        }

        public void Dispose()
        {
            _cleanupTimer.Dispose();
        }
    }
}
