using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using QuazalWV;
using System.Drawing;

namespace AcbRdv
{
    public static class OnlineConfigService
    {
        public static readonly object _sync = new object();
        public static bool _exit = false;
        private static TcpListener listener;
        private static string ip = Global.ServerBindAddress;
        private static readonly ushort listenPort = 80;
        private static readonly ushort targetPort = 21030;

        public static void Start()
        {
            _exit = false;
            new Thread(tMainThread).Start();
        }

        public static void Stop()
        {
            lock (_sync)
            {
                _exit = true;
            }
            if (listener != null)
                listener.Stop();
        }

        public static void tMainThread(object obj)
        {
            listener = new TcpListener(IPAddress.Any, listenPort);
            listener.Start();
            Log.WriteLine(1, "Server started", LogSource.OnlineConfigSvc);
            while (true)
            {
                lock (_sync)
                {
                    if (_exit)
                        break;
                }
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    new Thread(tClientHandler).Start(client);
                }
                catch { }
            }
            Log.WriteLine(1, "Server stopped", LogSource.OnlineConfigSvc);
        }

        public static void tClientHandler(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream ns = client.GetStream();
            MemoryStream m = new MemoryStream();
            // required for local testing
            Thread.Sleep(300);
            while (ns.DataAvailable)
                m.WriteByte((byte)ns.ReadByte());
            Log.WriteLine(2, $"Received {m.Length} bytes", LogSource.OnlineConfigSvc);
            string content;
            m.Position = 0;
            using (var reader = new StreamReader(m))
            {
                content = reader.ReadToEnd();
            }
            string httpEndpoint = content.Split('?')[0];
            string httpRequest = content.Split('\n')[0].Replace("\r", "");
            StringBuilder sb = new StringBuilder();
            // 404
            if (httpEndpoint != "GET /OnlineConfigService.svc/GetOnlineConfig")
            {
                var ep = client.Client.RemoteEndPoint as IPEndPoint;
                Log.WriteLine(1, $"[{ep.Address}:{ep.Port}] {httpRequest}", LogSource.OnlineConfigSvc, Color.Red);
                MakeResponse404(sb);
                Send(ns, sb);
                return;
            }
            // 200
            Log.WriteLine(1, "Client connected", LogSource.OnlineConfigSvc, Global.DarkTheme ? Color.LimeGreen : Color.Green);
            Log.WriteLine(2, httpRequest, LogSource.OnlineConfigSvc, Global.DarkTheme ? Color.LimeGreen : Color.Green);
            sb.Append("[");
            int count = 0;
            foreach (KeyValuePair<string, string> pair in responseData)
            {
                sb.Append("{\"Name\":\"" + pair.Key + "\",");
                sb.Append("\"Values\":[\"" + pair.Value.Replace("#IP#", ip).Replace("#PORT#", targetPort.ToString()) + "\"]}");
                if (count++ < 7)
                    sb.Append(",");
            }
            sb.Append("]");
            
            StringBuilder sb2 = new StringBuilder();
            MakeResponse200(sb2, sb.Length);
            sb2.Append(sb.ToString());
            Send(ns, sb2);
        }

        private static void MakeResponse200(StringBuilder sb, int contentLen)
        {
            sb.AppendLine("HTTP/1.1 200 OK");
            sb.AppendLine("Cache-Control: private");
            sb.AppendLine($"Content-Length: {contentLen}");
            sb.AppendLine("Content-Type: application/json; charset=utf-8");
            sb.AppendLine("Server: Microsoft-IIS/7.5");
            sb.AppendLine("X-AspNet-Version: 2.0.50727");
            sb.AppendLine("X-Powered-By: ASP.NET");
            sb.AppendLine("Date: Fri, 01 Nov 2019 14:04:13 GMT");
            sb.AppendLine("");
        }

        private static void MakeResponse404(StringBuilder sb)
        {
            sb.AppendLine("HTTP/1.1 404 Not Found");
            sb.AppendLine("Cache-Control: private");
            sb.AppendLine("Content-Length: 0");
            sb.AppendLine("Content-Type: deez/nuts; charset=utf-8");
            sb.AppendLine("X-Powered-By: ur mom");
            sb.AppendLine("Date: Tue, 11 Sep 2001 13:46:00 GMT");
            sb.AppendLine("");
        }

        private static void Send(NetworkStream ns, StringBuilder sb)
        {
            try
            {
                byte[] buff = Encoding.ASCII.GetBytes(sb.ToString());
                ns.Write(buff, 0, buff.Length);
                ns.Flush();
            }
            catch (IOException ex)
            {
                Log.WriteLine(1, $"IOException when sending to client: {ex.Message}", LogSource.OnlineConfigSvc, Color.Red);
            }
            catch (SocketException ex)
            {
                Log.WriteLine(1, $"SocketException when sending to client: {ex.Message}", LogSource.OnlineConfigSvc, Color.Red);
            }
            finally
            {
                try { ns?.Close(); } catch { }
            }
        }

        private static Dictionary<string, string> responseData = new Dictionary<string, string>()
        {
            {"SandboxUrl", @"prudp:\/address=#IP#;port=#PORT#"},
            {"SandboxUrlWS", @"#IP#:#PORT#"},
            {"uplay_DownloadServiceUrl", @"#IP#\/UplayServices\/UplayFacade\/DownloadServicesRESTXML.svc\/REST\/XML\/?url="},
            {"uplay_DynContentBaseUrl", @"#IP#\/u\/Uplay\/"},
            {"uplay_DynContentSecureBaseUrl", @"#IP#\/"},
            {"uplay_LinkappBaseUrl", @"#IP#\/u\/Uplay\/Packages\/linkapp\/1.1\/"},
            {"uplay_PackageBaseUrl", @"#IP#\/u\/Uplay\/Packages\/1.0.1\/"},
            {"uplay_WebServiceBaseUrl", @"#IP#\/UplayServices\/UplayFacade\/ProfileServicesFacadeRESTXML.svc\/REST\/"},
        };
    }
}
