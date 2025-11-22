using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace QuazalWV
{
    public static class Log
    {
        public static RichTextBox box = null;
        public static int MinPriority = 10; //1..10 1=less, 10=all
        public static string logFileName = "log.txt";
        public static string logPacketsFileName = "packetLog.bin";
        public static readonly object _sync = new object();
        public static readonly object _filesync = new object();
        private static readonly BlockingCollection<LogWorkItem> logQueue = new BlockingCollection<LogWorkItem>();
        private static Thread logWorker;
        public static bool enablePacketLogging = true;

        public static void ClearLog()
        {
            if (File.Exists(logFileName))
                File.Delete(logFileName);
            if (File.Exists(logPacketsFileName))
                File.Delete(logPacketsFileName);
            lock (_sync)
            {
                while (logQueue.TryTake(out _)) { }
            }
        }

        public static void WriteLine(int priority, string content, LogSource source = LogSource.Undefined, object color = null, ClientInfo client = null, bool skipSpace = false)
        {
            if (box == null) return;
            try
            {
                box.Invoke(new Action(delegate
                {
                    string line = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " : [" + priority.ToString("D2") + "]";
                    if (source != LogSource.Undefined)
                        line += $"[{source}]";
                    if (client != null && client.User != null && client.User.Name != null)
                        line += $"[{client.User.Name}]";
                    if (line.Length > 0 && line[line.Length - 1] != ' ' && !skipSpace)
                        line += " ";
                    if (priority <= MinPriority)
                    {
                        Color c;
                        if (color != null)
                            c = (Color)color;
                        else
                            c = Color.Black;
                        if (content.ToLower().Contains("error"))
                            c = Color.Red;
                        box.SelectionStart = box.TextLength;
                        box.SelectionLength = 0;
                        box.SelectionColor = c;
                        box.AppendText(line + content + "\n");
                        box.SelectionColor = box.ForeColor;
                        box.ScrollToCaret();                        
                    }
                    EnqueueLog(line + content + "\n", null);
                }));
            }
            catch { }
        }

        public static void WriteRmcLine(int priority, string content, RMCP.PROTOCOL protocol, LogSource source = LogSource.RMC, object color = null, ClientInfo client = null)
        {
            if (box == null) return;
            try
            {
                box.Invoke(new Action(delegate
                {
                    string line = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " : [" + priority.ToString("D2") + "]";
                    line += $"[{source} {protocol}]";
                    if (client != null && client.User != null && client.User.Name != null)
                        line += $"[{client.User.Name}]";
                    line += " ";
                    if (priority <= MinPriority)
                    {
                        Color c;
                        if (color != null)
                            c = (Color)color;
                        else
                            c = Color.Black;
                        if (content.ToLower().Contains("error"))
                            c = Color.Red;
                        box.SelectionStart = box.TextLength;
                        box.SelectionLength = 0;
                        box.SelectionColor = c;
                        box.AppendText(line + content + "\n");
                        box.SelectionColor = box.ForeColor;
                        box.ScrollToCaret();
                    }
                    EnqueueLog(line + content + "\n", null);
                }));
            }
            catch { }
        }

        public static string MakeDetailedPacketLog(byte[] data, bool isSinglePacket = false)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                PrudpPacket qp = new PrudpPacket(data);
                sb.AppendLine("##########################################################");
                sb.AppendLine(qp.ToStringDetailed());
                if (qp.type == PrudpPacket.PACKETTYPE.DATA && qp.m_byPartNumber == 0)
                {
                    switch (qp.m_oSourceVPort.type)
                    {
                        case PrudpPacket.STREAMTYPE.OldRVSec:
                            if (qp.flags.Contains(PrudpPacket.PACKETFLAG.FLAG_ACK))
                                break;
                            sb.AppendLine("Trying to process RMC packet...");
                            try
                            {
                                MemoryStream m = new MemoryStream(qp.payload);
                                RMCP p = new RMCP(qp);
                                m.Seek(p._afterProtocolOffset + 4, 0);
                                if (!p.isRequest)
                                    m.ReadByte();
                                p.methodID = Helper.ReadU32(m);
                                sb.AppendLine("\tRMC Request  : " + p.isRequest);
                                sb.AppendLine("\tRMC Protocol : " + p.proto);
                                sb.AppendLine("\tRMC Method   : " + p.methodID.ToString("X"));
                                if (p.proto == RMCP.PROTOCOL.GlobalNotificationEvent && p.methodID == 1)
                                {
                                    sb.AppendLine("\t\tNotification :");
                                    sb.AppendLine("\t\t\tSource".PadRight(20) + ": 0x" + Helper.ReadU32(m).ToString("X8"));
                                    uint type = Helper.ReadU32(m);
                                    sb.AppendLine("\t\t\tType".PadRight(20) + ": " + (type / 1000));
                                    sb.AppendLine("\t\t\tSubType".PadRight(20) + ": " + (type % 1000));
                                    sb.AppendLine("\t\t\tParam 1".PadRight(20) + ": 0x" + Helper.ReadU32(m).ToString("X8"));
                                    sb.AppendLine("\t\t\tParam 2".PadRight(20) + ": 0x" + Helper.ReadU32(m).ToString("X8"));
                                    sb.AppendLine("\t\t\tParam String".PadRight(20) + ": " + Helper.ReadString(m));
                                    sb.AppendLine("\t\t\tParam 3".PadRight(20) + ": 0x" + Helper.ReadU32(m).ToString("X8"));
                                }
                                sb.AppendLine();
                            }
                            catch
                            {
                                sb.AppendLine("Error processing RMC packet");
                                sb.AppendLine();
                            }
                            break;
                        case PrudpPacket.STREAMTYPE.DO:
                            sb.AppendLine("ACB RDV cannot process DO protocol packets");
                            break;
                    }
                }
                int size2 = qp.ToBuffer().Length;
                if (size2 == data.Length || isSinglePacket)
                    break;
                MemoryStream m2 = new MemoryStream(data);
                m2.Seek(size2, 0);
                size2 = (int)(m2.Length - m2.Position);
                if (size2 <= 8)
                    break;
                data = new byte[size2];
                m2.Write(data, 0, size2);
            }
            return sb.ToString();
        }

        public static void LogPacket(bool sent, byte[] data)
        {
            if (!enablePacketLogging)
                return;
            MemoryStream m = new MemoryStream();
            m.WriteByte(1); // version
            m.WriteByte((byte)(sent ? 1 : 0));
            Helper.WriteU32(m, (uint)data.Length);
            m.Write(data, 0, data.Length);
            EnqueueLog(null, m.ToArray());
        }

        private static void EnsureLogWorker()
        {
            if (logWorker != null)
                return;
            lock (_sync)
            {
                if (logWorker != null)
                    return;
                logWorker = new Thread(ProcessLogQueue)
                {
                    IsBackground = true,
                    Name = "LogWriter"
                };
                logWorker.Start();
            }
        }

        private static void EnqueueLog(string text, byte[] packet)
        {
            EnsureLogWorker();
            logQueue.Add(new LogWorkItem { Text = text, Packet = packet });
        }

        private static void ProcessLogQueue()
        {
            foreach (var item in logQueue.GetConsumingEnumerable())
            {
                try
                {
                    if (!string.IsNullOrEmpty(item.Text))
                    {
                        lock (_filesync)
                        {
                            File.AppendAllText(logFileName, item.Text);
                        }
                    }

                    if (item.Packet != null)
                    {
                        lock (_filesync)
                        {
                            using (FileStream fs = new FileStream(logPacketsFileName, FileMode.Append, FileAccess.Write, FileShare.Read))
                            {
                                fs.Write(item.Packet, 0, item.Packet.Length);
                                fs.Flush();
                            }
                        }
                    }
                }
                catch
                {
                    // swallow logging errors to avoid crashing callers
                }
            }
        }

        private class LogWorkItem
        {
            public string Text { get; set; }
            public byte[] Packet { get; set; }
        }
    }
}
