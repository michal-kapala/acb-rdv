using System.Collections.Generic;
using System.Net;
using System.Configuration;
using System.Drawing;

namespace QuazalWV
{
    public static class Global
    {
        public const string Rc4KeyRdv = "CD&ML";
        public static readonly byte[] Rc4KeyP2p = Helper.P2pKey();
        public static string ServerBindAddress { get; set; } = ConfigurationManager.AppSettings["SecureServerAddress"];
        public static uint IdCounter { get; set; } = 0x12345678;
        public static uint PidCounter { get; set; } = 0x1234;
        public static uint GathIdCounter { get; set; } = 0x34;
        public static List<ClientInfo> Clients { get; set; } = new List<ClientInfo>();
        public static uint NextGameSessionId { get; set; } = 1;
        public static List<Session> Sessions { get; set; } = new List<Session>();

        public static ClientInfo GetClientByEndPoint(IPEndPoint ep)
        {
            foreach (ClientInfo c in Clients)
                if (c.ep.Address.ToString() == ep.Address.ToString() && c.ep.Port == ep.Port)
                    return c;
            WriteLog(2, $"Cant find client for endpoint: {ep}");
            return null;
        }

        public static ClientInfo GetClientByIDrecv(uint id)
        {
            foreach (ClientInfo c in Clients)
                if (c.IDrecv == id)
                    return c;
            WriteLog(2, "Cant find client for id : 0x" + id.ToString("X8"));
            return null;
        }

        private static void WriteLog(int priority, string content)
        {
            Log.WriteLine(priority, content, LogSource.Global, Color.Orange);
        }

        internal static void RemoveSessionsOnLogin(ClientInfo client)
        {
            client.RegisteredUrls.Clear();
            client.Urls.Clear();
            Sessions.RemoveAll(s => s.HostPid == client.User.Pid);
        }
    }
}
