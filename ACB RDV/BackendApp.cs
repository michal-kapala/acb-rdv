using System;
using System.Configuration;
using System.Windows.Forms;
using QuazalWV;
using System.Drawing;

namespace AcbRdv
{
    public partial class BackendApp : Form
    {
        public BackendApp()
        {
            InitializeComponent();
            Log.ClearLog();
            Log.box = richTextBox1;
            DbHelper.Init();
            toolStripComboBox1.SelectedIndex = 0;
            // Register the Load event handler
            this.Load += BackendApp_Load;
        }

        private void BackendApp_Load(object sender, EventArgs e)
        {
            // Parse the autostart value from App.config
            bool autostart = false;
            string autostartStr = ConfigurationManager.AppSettings["Autostart"];
            if (!string.IsNullOrEmpty(autostartStr))
            {
                bool.TryParse(autostartStr, out autostart);
            }

            // If autostart is enabled, start the servers after the form has loaded
            if (autostart)
            {
                // Call the button click handler to start services
                toolStripButton1_Click(this, EventArgs.Empty);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            OnlineConfigService.Start();
            RdvServer.Start();
            AuthServer.Start();           
            toolStripButton1.Enabled = false;
            toolStripButton2.Enabled = true;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            OnlineConfigService.Stop();
            RdvServer.Stop();
            AuthServer.Stop();
            toolStripButton1.Enabled = true;
            toolStripButton2.Enabled = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            OnlineConfigService.Stop();
            RdvServer.Stop();
            AuthServer.Stop();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            new DecryptTool().Show();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            new LogFilter().Show();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (toolStripComboBox1.SelectedIndex)
            {
                default:
                case 0:
                    Log.MinPriority = 1;
                    break;
                case 1:
                    Log.MinPriority = 2;
                    break;
                case 2:
                    Log.MinPriority = 5;
                    break;
                case 3:
                    Log.MinPriority = 10;
                    break;
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            new PacketGenerator().Show();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            new UDPProcessor().Show();
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            new SendNotification().Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            NotificationQueue.Update();
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            Log.enablePacketLogging = toolStripButton9.Checked;
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            var sessions = Global.Sessions;
            if (sessions.Count == 0)
            {
                WriteLog(1, "No active sessions found.", Color.Gray);
                return;
            }
            WriteLog(1, $"Active sessions ({sessions.Count}):", Color.Black);
            foreach (var session in sessions)
            {
                WriteLog(1, session.ToString(), Color.DarkBlue);
            }
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            if (Global.AllowPrivateSessions)
            {
                Global.AllowPrivateSessions = false;
                WriteLog(1, "Private sessions are now PERMITTED.", Color.OrangeRed);
            }
            else
            {
                Global.AllowPrivateSessions = true;
                WriteLog(1, "Private sessions are now ALLOWED.", Color.Gray);
            }
        }

        private static void WriteLog(int priority, string content, Color color)
        {
            Log.WriteLine(priority, content, LogSource.BackendApp, color);
        }
    }
}
