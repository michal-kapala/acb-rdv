using System.Collections.Generic;
using System.Net;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;
using System;

namespace QuazalWV
{
    public static class Global
    {
        public const string Rc4KeyRdv = "CD&ML";
        public static readonly byte[] Rc4KeyP2p = Helper.P2pKey();
        private static string _serverBindAddressConfig = ConfigurationManager.AppSettings["SecureServerAddress"];
        public static string ServerBindAddress { get; private set; }
        public static bool IsPrivate { get; private set; }
        public static uint IdCounter { get; set; } = 0x12345678;
        public static uint PidCounter { get; set; } = 0x1234;
        public static uint GathIdCounter { get; set; } = 0x34;
        public static List<ClientInfo> Clients { get; set; } = new List<ClientInfo>();
        public static uint NextGameSessionId { get; set; } = 1;
        public static List<Session> Sessions { get; set; } = new List<Session>();
        public static bool AllowPrivateSessions { get; set; } = true;
        public static bool DarkTheme { get; set; } = false;

        // Static constructor: resolve server address on startup
        static Global()
        {
            string darkThemeStr = ConfigurationManager.AppSettings["DarkTheme"];
            if (!string.IsNullOrEmpty(darkThemeStr))
            {
                bool.TryParse(darkThemeStr, out bool darkTheme);
                DarkTheme = darkTheme;
            }
            try
            {
                ServerBindAddress = ResolveAddress(_serverBindAddressConfig);
                IsPrivate = CheckIfPrivate(ServerBindAddress);
                if (IsPrivate)
                {
                    WriteHostLog(1, $"ACB Server is PRIVATE. Clients will communicate via LAN.", Color.DarkOrange);
                }
                else
                {
                    WriteHostLog(1, $"ACB Server is PUBLIC. Clients will communicate via WAN.", Color.DarkCyan);
                }
            }   
            catch (ConfigurationErrorsException ex)
                {
                    ShowConfigError(ex);
                }
                catch (Exception ex)
                {
                    ShowGeneralError(ex);
                }
        }

        // Resolves an IP or hostname to an IPv4 string for server binding
        private static string ResolveAddress(string addressOrHost)
        {
            if (string.IsNullOrWhiteSpace(addressOrHost))
                throw new ConfigurationErrorsException("SecureServerAddress is empty in App.config.");

            // Strict IPv4 validation using octets
            string[] parts = addressOrHost.Split('.');
            if (parts.Length == 4)
            {
                bool valid = true;
                foreach (string part in parts)
                {
                    if (!int.TryParse(part, out int octet) || octet < 0 || octet > 255)
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                {
                    if (addressOrHost == "0.0.0.0") // Doesn't receive any connections
                    {
                        throw new ConfigurationErrorsException("The IP address '0.0.0.0' is not supported. Please specify a different IPv4 address.");
                    }

                    WriteHostLog(1, $"SecureServerAddress is a valid IPv4: '{addressOrHost}'", Global.DarkTheme ? Color.RoyalBlue : Color.Blue);
                    return addressOrHost;
                }
            }

            // Not a valid IPv4, try to resolve as hostname
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(addressOrHost);
                foreach (IPAddress resolvedIp in entry.AddressList)
                {
                    if (resolvedIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) // IPv4 only
                    {
                        if (resolvedIp.ToString() == "0.0.0.0") // Doesn't receive any connections
                        {
                            throw new ConfigurationErrorsException("The resolved IP '0.0.0.0' is not supported. Please configure a different IPv4 address.");
                        }

                        WriteHostLog(1, $"Hostname '{addressOrHost}' resolved to IPv4 '{resolvedIp}'", Global.DarkTheme ? Color.RoyalBlue : Color.Blue);
                        return resolvedIp.ToString();
                    }
                }
                throw new ConfigurationErrorsException($"Hostname '{addressOrHost}' could not be resolved to any IPv4 address.");
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException($"Failed to resolve hostname '{addressOrHost}': {ex.Message}");
            }
        }

        // Check if IP is private/local
        public static bool CheckIfPrivate(string ipString)
        {
            if (IPAddress.TryParse(ipString, out IPAddress ip))
            {
                byte[] bytes = ip.GetAddressBytes();

                // 10.0.0.0/8
                if (bytes[0] == 10)
                    return true;

                // 172.16.0.0 – 172.31.255.255
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                    return true;

                // 192.168.0.0/16
                if (bytes[0] == 192 && bytes[1] == 168)
                    return true;

                // Loopback 127.0.0.0/8
                if (bytes[0] == 127)
                    return true;

                // Link-local 169.254.0.0/16
                if (bytes[0] == 169 && bytes[1] == 254)
                    return true;
            }
            return false;
        }

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

        #region Logging helpers
        private static void WriteLog(int priority, string content)
        {
            Log.WriteLine(priority, content, LogSource.Global, Color.Orange);
        }

        private static void WriteHostLog(int priority, string content, Color color)
        {
            Log.WriteLine(priority, content, LogSource.Global, color);
        }

        #endregion
        internal static void RemoveSessionsOnLogin(ClientInfo client)
        {
            client.RegisteredUrls.Clear();
            client.Urls.Clear();
            Sessions.RemoveAll(s => s.HostPid == client.User.Pid);
        }

        #region Error display helpers
        private static void ShowConfigError(ConfigurationErrorsException ex)
        {
            MessageBox.Show(
                ex.Message,
                "Configuration Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            Environment.Exit(1); // Exit application
        }

        private static void ShowGeneralError(Exception ex)
        {
            MessageBox.Show(
                "An unexpected error occurred:\n" + ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            Environment.Exit(1);
        }
        #endregion
    }
}
