using System.Collections.Generic;
using System.Net;
using System.Diagnostics;
using System.Configuration;
using System;

namespace QuazalWV
{
    public static class Global
    {
        public static readonly string keyDATA = "CD&ML";
        public static readonly string keyCheckSum = "8dtRv2oj";
        public static string serverBindAddress = ConfigurationManager.AppSettings["SecureServerAddress"];
        public static uint idCounter = 0x12345678;
        public static uint pidCounter = 0x1234;
        public static uint GathIdCounter { get; set; } = 0x34;
        public static string sessionURL = "prudp:/address=127.0.0.1;port=21032;RVCID=4660";
        public static List<ClientInfo> Clients { get; set; } = new List<ClientInfo>();
        public static Stopwatch uptime = new Stopwatch();
        public static uint NextGameSessionId { get; set; } = 1;
        public static List<Session> Sessions { get; set; } = new List<Session>();

        public static ClientInfo GetClientByEndPoint(IPEndPoint ep)
        {
            foreach (ClientInfo c in Clients)
                if (c.ep.Address.ToString() == ep.Address.ToString() && c.ep.Port == ep.Port)
                    return c;
            WriteLog(1, "Error : Cant find client for end point : " + ep.ToString());
            return null;
        }

        public static ClientInfo GetClientByIDsend(uint id)
        {
            foreach (ClientInfo c in Clients)
                if (c.IDsend == id)
                    return c;
            WriteLog(1, "Error : Cant find client for id : 0x" + id.ToString("X8"));
            return null;
        }

        public static ClientInfo GetClientByIDrecv(uint id)
        {
            foreach (ClientInfo c in Clients)
                if (c.ServerIncrementedGeneratedConnSignature == id)
                    return c;
            WriteLog(1, "Error : Cant find client for id : 0x" + id.ToString("X8"));
            return null;
        }

        public static bool MultiplayerEndpoint(IPEndPoint ep)
        {
            foreach (ClientInfo c in Clients)
                if (c.ep.Address.ToString() == ep.Address.ToString() && c.ep.Port != ep.Port)
                    return true;
            return false;
        }

        private static void WriteLog(int priority, string s)
        {
            Log.WriteLine(priority, "[Global] " + s);
        }

        internal static void RemoveSessionsOnLogin(ClientInfo client)
        {
            client.RegisteredUrls.Clear();
            client.Urls.Clear();
            Sessions.RemoveAll(s => s.HostPid == client.User.UserDBPid);
        }
        public static void RemovebySignature(uint client_servergenSignature)
        {
            List<uint> pids = new List<uint> { };
            foreach (ClientInfo client in Clients)
            {
                if (client.ServerIncrementedGeneratedConnSignature == client_servergenSignature)
                {
                    pids.Add(client.User.UserDBPid);
                }
            }
            if (pids.Count > 1)
            {
                Log.WriteLine(1, "There are multiple clients with the same IP this should not be hapening");
            }
            if (pids.Count == 0)
            {
                Log.WriteLine(1, "Unable to remove it is not present");
            }
            Clients.RemoveAll(item => item.ServerIncrementedGeneratedConnSignature == client_servergenSignature);
        }
        public static void RemovebyIP(IPAddress ipval)
        {
            List<uint> pids = new List<uint> { };
            foreach (ClientInfo client in Clients)
            {
                if (client.IPaddress == ipval)
                {
                    pids.Add(client.ServerIncrementedGeneratedConnSignature);
                }
            }
            if (pids.Count > 1)
            {
                Log.WriteLine(1, "There are multiple clients with the same IP this should not be hapening");
            }
            if (pids.Count == 0)
            {
                Log.WriteLine(1, "Unable to remove it is not present");
            }
            Clients.RemoveAll(item => item.IPaddress == ipval);


        }
    }
}
