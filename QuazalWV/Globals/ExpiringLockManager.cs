using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;
using System.Linq;

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
                    _locks.TryRemove(kvp.Key, out _);
                    // Find client associated with the endpoint
                    var client = Global.Clients.Find(c => c.ep != null && c.ep.Address.ToString() == kvp.Key.ToString());
                    if (client != null)
                    {
                        try
                        {
                            // Check if client is valid
                            bool isValid = client.User != null && client.ep != null;
                            Log.WriteLine(1, $"TIMEOUT{(isValid ? "" : " for invalid client")}", LogSource.PRUDP, Color.Gray, client);

                            if (isValid)
                            {
                                // Remove the client's PID from all sessions
                                foreach (var session in Global.Sessions.ToList())
                                {
                                    session.PublicPids.RemoveAll(pid => pid == client.User.Pid);
                                    session.PrivatePids.RemoveAll(pid => pid == client.User.Pid);
                                    // Remove session if no participants remain
                                    if (session.NbParticipants() == 0)
                                    {
                                        Global.Sessions.Remove(session);
                                        Log.WriteLine(1, $"Session {session.Key.SessionId} deleted due to TIMEOUT from player {client.User.Pid}", LogSource.PRUDP, Color.Gray, client);
                                    }
                                }
                                // Notify friends if the client is valid
                                var rels = DbHelper.GetRelationships(client.User.Pid, (byte)PlayerRelationship.Friend);
                                foreach (var relationship in rels)
                                {
                                    uint friendPid = relationship.RequesterPid == client.User.Pid ? relationship.RequesteePid : relationship.RequesterPid;
                                    ClientInfo friend = Global.Clients.Find(c => c.User != null && c.User.Pid == friendPid);
                                    if (friend != null)
                                        NotificationManager.FriendStatusChanged(friend, client.User.Pid, client.User.Name, false);
                                }
                            }
                            // Remove client independently of validity
                            Global.Clients.Remove(client);
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLine(1, $"Error during client timeout cleanup: {ex}", LogSource.PRUDP, Color.Red);
                            // Remove the client anyway
                            if (Global.Clients.Contains(client))
                                Global.Clients.Remove(client);
                        }
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
