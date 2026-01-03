using QuazalWV;
using System;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading;

namespace AcbRdv
{
    public static class StatusAPI
    {
        public static readonly object _sync = new object();
        public static bool _exit = false;
        public static ushort listenPort = 21032;
        public static HttpListener listener;

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
            listener?.Close();
        }

        public static void tMainThread(object obj)
        {
            WriteLog(1, "Server started");

            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add($"http://+:{listenPort}/");
                listener.Start();
            }
            catch (Exception ex)
            {
                WriteLog(1, $"Listener Error: {ex.Message}", Color.Red);
                return;
            }

            while (true)
            {
                lock (_sync)
                {
                    if (_exit)
                        break;
                }

                try
                {
                    var context = listener.GetContext();
                    LogRequest(context); // log every request with new priorities/colors
                    ProcessRequest(context);
                }
                catch (Exception ex)
                {
                    WriteLog(1, $"HTTP Error: {ex.Message}", Color.Red);
                }
            }

            WriteLog(1, "Server stopped");
        }

        private static void ProcessRequest(HttpListenerContext ctx)
        {
            string path = ctx.Request.Url.AbsolutePath.ToLower();

            switch (path)
            {
                case "/":
                    HandleRoot(ctx);
                    return;

                case "/sessions":
                    HandleSessionsPlainText(ctx);
                    return;

                case "/clients":
                    HandleClientsPlainText(ctx);
                    return;

                default:
                    ctx.Response.StatusCode = 404;
                    ctx.Response.Close();
                    return;
            }
        }

        /// <summary>
        /// Root endpoint for health checks
        /// </summary>
        private static void HandleRoot(HttpListenerContext ctx)
        {
            string content = $"Status: OK";
            WriteResponse(ctx, content, "text/plain");
        }

        /// <summary>
        /// Returns ALL sessions as clean human-readable text (Host URLs removed)
        /// </summary>
        private static void HandleSessionsPlainText(HttpListenerContext ctx)
        {
            var sessions = Global.Sessions;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Total Sessions: {sessions.Count}");
            sb.AppendLine();

            bool any = false;

            foreach (var session in sessions)
            {
                if (session?.GameSession == null)
                    continue;

                any = true;

                // Remove Host URLs block for security
                string text = session.ToString();
                int hostUrlsIndex = text.IndexOf("\t[Host URLs]");
                if (hostUrlsIndex >= 0)
                    text = text.Substring(0, hostUrlsIndex);

                // Split into lines
                var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    // Only indent lines that are part of Attributes
                    if (line.StartsWith("\t\t") || line.StartsWith("\t\t\t"))
                    {
                        line = line.Replace("\t", "    ");
                    }
                    else if (line.StartsWith("\t"))
                    {
                        line = line.Substring(1); // remove single leading tab for top-level info
                    }
                    sb.AppendLine(line);
                }

                sb.AppendLine(); // extra newline between sessions
            }

            if (!any)
                sb.AppendLine("No active sessions.");

            WriteResponse(ctx, sb.ToString(), "text/plain");
        }

        /// <summary>
        /// Returns ALL clients as readable plain text
        /// </summary>
        private static void HandleClientsPlainText(HttpListenerContext ctx)
        {
            var clients = Global.Clients;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Total Clients: {clients.Count}");
            sb.AppendLine();

            if (clients.Count == 0)
            {
                sb.AppendLine("No clients online.");
                WriteResponse(ctx, sb.ToString(), "text/plain");
                return;
            }

            foreach (var c in clients)
            {
                sb.AppendLine($"[PID: {c.User?.Pid ?? 0}]");
                sb.AppendLine($"[Locale: {c.LocaleCode ?? "N/A"}]");
                sb.AppendLine($"[In Game: {c.InGameSession}]");
                sb.AppendLine($"[Session ID: {c.GameSessionID}]");
                sb.AppendLine();
            }

            WriteResponse(ctx, sb.ToString(), "text/plain");
        }

        private static void WriteResponse(HttpListenerContext ctx, string content, string contentType)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);

            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = contentType;
            ctx.Response.ContentEncoding = Encoding.UTF8;
            ctx.Response.ContentLength64 = buffer.Length;

            ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
            ctx.Response.OutputStream.Close();
        }

        /// <summary>
        /// Logs every HTTP request
        /// </summary>
        private static void LogRequest(HttpListenerContext ctx)
        {
            string ipPort = ctx.Request.RemoteEndPoint.ToString();
            string method = ctx.Request.HttpMethod;
            string path = ctx.Request.Url.PathAndQuery;

            int priority;
            Color color;

            switch (path.ToLower())
            {
                case "/":
                case "/sessions":
                case "/clients":
                    priority = 1;
                    color = Global.DarkTheme ? Color.LimeGreen : Color.Green;
                    break;

                default:
                    priority = 1;
                    color = Color.Red;
                    break;
            }

            WriteLog(priority, $"[{ipPort}] {method} {path}", color);
        }

        private static void WriteLog(int priority, string content, Color? color = null)
        {
            Log.WriteLine(priority, content, LogSource.StatusAPI, color);
        }
    }
}
