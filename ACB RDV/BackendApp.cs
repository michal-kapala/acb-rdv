using QuazalWV;
using System;
using System.Reflection.Metadata;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

namespace AcbRdv
{
    public partial class BackendApp : Form
    {
        public BackendApp()
        {
            InitializeComponent();
            Log.ClearLog();
            Log.Write = WriteLogBox;
            DbHelper.Init();
            toolStripComboBox1.SelectedIndex = 0;
        }

        private void WriteLogBox((string, Color) tuple)
        {
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.SelectionLength = 0;
            richTextBox1.SelectionColor = tuple.Item2;
            richTextBox1.AppendText(tuple.Item1);
            richTextBox1.SelectionColor = richTextBox1.ForeColor;
            richTextBox1.ScrollToCaret();
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
    }
}
