using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;
using System.Net;
namespace QuazalWV
{
    public class ExpiringLockManager<TKey>
    {
        private class LockWrapper
        {
            public object LockObject { get; } = new object();
            public DateTime LastUsed { get; set; } = DateTime.UtcNow;
        }

        private readonly ConcurrentDictionary<TKey, LockWrapper> _locks = new ConcurrentDictionary<TKey, LockWrapper>();
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
                    Log.WriteLine(1, $"[PRUDP LockManager] Removed lock which was no longer used {kvp.Key}", Color.Red);
                    bool success = false;
                    try
                    {
                        Global.RemovebyIP(kvp.Key.ToString());
                        success=_locks.TryRemove(kvp.Key, out _);
                       
                    }
                    catch(Exception e)
                    {
                        Log.WriteLine(1, $"[PRUDP LockManager] Error Removing key {e}", Color.Red);
                    }
                    if (success==false)
                    {
                        Log.WriteLine(1, $"[PRUDP LockManager] got status {success} when trying to remove a lock", Color.Red);
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
