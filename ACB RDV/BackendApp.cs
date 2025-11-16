using System;
using System.Configuration;
using System.Windows.Forms;
using QuazalWV;
using System.Drawing;
using System.Runtime.InteropServices;

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
            // Apply design overrides
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (Global.DarkTheme)
            {
                DarkThemeManager.ApplyDarkTheme(this);
            }
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
            WriteLog(1, $"Active sessions ({sessions.Count}):", Global.DarkTheme ? Color.White : Color.Black);
            foreach (var session in sessions)
            {
                WriteLog(1, session.ToString(), Global.DarkTheme ? Color.RoyalBlue : Color.Blue);
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

        public static class DarkThemeManager
        {
            public static void ApplyDarkTheme(Control control)
            {
                // Apply immersive dark mode once per form
                if (control is Form form)
                    DarkWindowAttribute.Enable(form, Global.DarkTheme);

                Color textColor = Color.White;
                Color darkBg = Color.FromArgb(30, 30, 30);
                Color toolstripBg = Color.FromArgb(20, 20, 20);
                Color toolstripBorder = Color.FromArgb(80, 80, 80);
                Color inputBg = Color.FromArgb(50, 50, 50);
                Color separatorColor = Color.FromArgb(125, 125, 125);
                Color buttonhoverColor = Color.FromArgb(0, 140, 250);

                // Apply general background
                control.BackColor = darkBg;

                // Adjust input fields
                if (control is TextBox tb)
                {
                    tb.BackColor = inputBg;
                    tb.ForeColor = textColor;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (control is RichTextBox rtb)
                {
                    rtb.BackColor = darkBg;
                    rtb.BorderStyle = BorderStyle.None;
                }
                else
                {
                    // All other controls
                    control.ForeColor = textColor;
                }

                // Apply custom renderer for ToolStrips
                if (control is ToolStrip ts)
                {
                    ts.BackColor = toolstripBg;
                    ts.ForeColor = textColor;

                    // Only assign renderer once to avoid recursive re-assignment
                    if (!(ts.Renderer is DarkToolStripRenderer))
                        ts.Renderer = new DarkToolStripRenderer(darkBg, buttonhoverColor, textColor, separatorColor, toolstripBorder);
                }

                // Recursively apply theme to all child controls
                foreach (Control child in control.Controls)
                    ApplyDarkTheme(child);
            }

            // Custom ToolStrip renderer for dark theme styling, hover effects, and separators
            private class DarkToolStripRenderer : ToolStripProfessionalRenderer
            {
                private readonly Color normalBg;
                private readonly Color hoverBg;
                private readonly Color textColor;
                private readonly Color buttonBorderColor;
                private readonly Color stripBorderColor;
                private readonly Color separatorColor;

                public DarkToolStripRenderer(Color normalBg, Color hoverBg, Color textColor, Color separatorColor, Color toolstripBorder)
                {
                    this.normalBg = normalBg;
                    this.hoverBg = hoverBg;
                    this.textColor = textColor;
                    this.separatorColor = separatorColor;

                    buttonBorderColor = hoverBg;                // Border for checked buttons
                    stripBorderColor = toolstripBorder;         // Border around the ToolStrip area
                }

                // Draw button background (handles hover and active states)
                protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
                {
                    Color bg;

                    // Use item's own BackColor if set
                    if (e.Item.BackColor != Color.Empty && e.Item.BackColor != SystemColors.Control)
                        bg = e.Item.BackColor;
                    else
                        bg = normalBg;

                    // Hover state
                    if (e.Item.Selected)
                        bg = hoverBg;

                    using (SolidBrush brush = new SolidBrush(bg))
                        e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));

                    // Draw border for checked buttons
                    if (e.Item is ToolStripButton btn && btn.Checked)
                    {
                        using (Pen pen = new Pen(buttonBorderColor))
                        {
                            Rectangle rect = new Rectangle(0, 0, e.Item.Width - 1, e.Item.Height - 1);
                            e.Graphics.DrawRectangle(pen, rect);
                        }
                    }
                }

                // Apply consistent text color
                protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
                {
                    if (e.Item.BackColor != Color.Empty && e.Item.BackColor != SystemColors.Control)
                    {
                        e.TextColor = e.Item.ForeColor != Color.Empty ? e.Item.ForeColor : textColor;
                    }
                    else
                    {
                        e.TextColor = textColor;
                    }

                    base.OnRenderItemText(e);
                }

                // Draw border around the entire ToolStrip
                protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
                {
                    using (Pen borderPen = new Pen(stripBorderColor))
                    {
                        Rectangle rect = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
                        e.Graphics.DrawRectangle(borderPen, rect);
                    }
                }

                // Draw separators with a slightly dark gray color
                protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
                {
                    using (Pen pen = new Pen(separatorColor))
                    {
                        Rectangle rect = e.Item.ContentRectangle;

                        if (e.Vertical)
                        {
                            // add small padding to replicate the default design
                            int x = rect.Left + rect.Width / 2;
                            e.Graphics.DrawLine(pen, x, rect.Top + 2, x, rect.Bottom - 2);
                        }
                        else
                        {
                            // add small padding to replicate the default design
                            int y = rect.Top + rect.Height / 2;
                            e.Graphics.DrawLine(pen, rect.Left + 2, y, rect.Right - 2, y);
                        }
                    }
                }
            }

            // Enables Windows immersive dark mode (Windows 10/11 compatible)
            public static class DarkWindowAttribute
            {
                [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
                private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
                private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
                private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
                public static void Enable(Form form, bool enable)
                {
                    if (form == null) return;
                    int useDark = enable ? 1 : 0;
                    try
                    {
                        DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int));
                        DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useDark, sizeof(int));
                    }
                    catch
                    {
                        // Ignore if API is unavailable
                    }
                }
            }
        }
    }
}
