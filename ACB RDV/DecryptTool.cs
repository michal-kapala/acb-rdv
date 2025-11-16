using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using QuazalWV;
using System.Drawing;
using static AcbRdv.BackendApp;

namespace AcbRdv
{
    public partial class DecryptTool : Form
    {
        string RmcKey { get; set; } = Global.Rc4KeyRdv;
        byte[] DoKey { get; set; } = Global.Rc4KeyP2p;
        bool UseRmcKey { get; set; } = true;

        public DecryptTool()
        {
            InitializeComponent();
            // Apply design overrides
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (Global.DarkTheme)
            {
                DarkThemeManager.ApplyDarkTheme(this);
                richTextBox1.ForeColor = Color.White;
                richTextBox2.ForeColor = Color.White;
            }
            radioButton1.Checked = true;
            radioButton2.Checked = false;
        }

        private byte[] ToBuff(string s)
        {
            MemoryStream m = new MemoryStream();
            for (int i = 0; i < s.Length; i += 2)
                m.WriteByte(Convert.ToByte(s.Substring(i, 2), 16));
            return m.ToArray();
        }

        private void DecryptDecompressButton_Click(object sender, EventArgs e)
        {
            try
            {
                string s = richTextBox1.Text.Trim();
                while (s.Contains(" "))
                    s = s.Replace(" ", "");
                if ((s.Length % 2) != 0)
                    return;
                byte[] data = ToBuff(s);
                data = UseRmcKey ? Helper.Decrypt(RmcKey, data) : Helper.Decrypt(DoKey, data);
                MemoryStream m = new MemoryStream();
                m.Write(data, 1, data.Length - 1);
                data = Helper.Decompress(m.ToArray());
                StringBuilder sb = new StringBuilder();
                foreach (byte b in data)
                    sb.Append(b.ToString("X2"));
                richTextBox2.Text = sb.ToString();
            }
            catch { richTextBox2.Text = "ERROR"; }
        }

        private void CompressEncryptButton_Click(object sender, EventArgs e)
        {
            try
            {
                string s = richTextBox2.Text.Trim();
                while (s.Contains(" "))
                    s = s.Replace(" ", "");
                if ((s.Length % 2) != 0)
                    return;
                byte[] data = ToBuff(s);
                uint sizeBefore = (uint)data.Length;
                byte[] buff = Helper.Compress(data);
                byte count = (byte)(sizeBefore / buff.Length);
                if ((sizeBefore % buff.Length) != 0)
                    count++;
                MemoryStream m = new MemoryStream();
                m.WriteByte(count);
                m.Write(buff, 0, buff.Length);
                data = UseRmcKey ? Helper.Encrypt(RmcKey, m.ToArray()) : Helper.Encrypt(DoKey, m.ToArray());
                StringBuilder sb = new StringBuilder();
                foreach (byte b in data)
                    sb.Append(b.ToString("X2"));
                richTextBox1.Text = sb.ToString();
            }
            catch { richTextBox1.Text = "ERROR"; }
        }

        private void DecryptButton_Click(object sender, EventArgs e)
        {
            try
            {
                string s = richTextBox1.Text.Trim();
                while (s.Contains(" "))
                    s = s.Replace(" ", "");
                if ((s.Length % 2) != 0)
                    return;
                byte[] data = ToBuff(s);
                data = UseRmcKey ? Helper.Decrypt(RmcKey, data) : Helper.Decrypt(DoKey, data);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in data)
                    sb.Append(b.ToString("X2"));
                richTextBox2.Text = sb.ToString();
            }
            catch { richTextBox2.Text = "ERROR"; }
        }

        private void EncryptButton_Click(object sender, EventArgs e)
        {
            try
            {
                string s = richTextBox2.Text.Trim();
                while (s.Contains(" "))
                    s = s.Replace(" ", "");
                if ((s.Length % 2) != 0)
                    return;
                byte[] data = ToBuff(s);
                data = UseRmcKey ? Helper.Encrypt(RmcKey, data) : Helper.Encrypt(DoKey, data);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in data)
                    sb.Append(b.ToString("X2"));
                richTextBox1.Text = sb.ToString();
            }
            catch { richTextBox1.Text = "ERROR"; }
        }

        private void DecompressButton_Click(object sender, EventArgs e)
        {
            try
            {
                string s = richTextBox1.Text.Trim();
                while (s.Contains(" "))
                    s = s.Replace(" ", "");
                if ((s.Length % 2) != 0)
                    return;
                byte[] data = ToBuff(s);
                MemoryStream m = new MemoryStream();
                m.Write(data, 1, data.Length - 1);
                data = Helper.Decompress(m.ToArray());
                StringBuilder sb = new StringBuilder();
                foreach (byte b in data)
                    sb.Append(b.ToString("X2"));
                richTextBox2.Text = sb.ToString();
            }
            catch { richTextBox2.Text = "ERROR"; }
        }

        private void CompressButton_Click(object sender, EventArgs e)
        {
            try
            {
                string s = richTextBox2.Text.Trim();
                while (s.Contains(" "))
                    s = s.Replace(" ", "");
                if ((s.Length % 2) != 0)
                    return;
                byte[] data = ToBuff(s);
                uint sizeBefore = (uint)data.Length;
                byte[] buff = Helper.Compress(data);
                byte count = (byte)(sizeBefore / buff.Length);
                if ((sizeBefore % buff.Length) != 0)
                    count++;
                MemoryStream m = new MemoryStream();
                m.WriteByte(count);
                m.Write(buff, 0, buff.Length);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in data)
                    sb.Append(b.ToString("X2"));
                richTextBox1.Text = sb.ToString();
            }
            catch { richTextBox1.Text = "ERROR"; }
        }

        private void RmcKeyRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            radioButton2.Checked = !radioButton1.Checked;
            UseRmcKey = true;
        }

        private void DoKeyRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            radioButton1.Checked = !radioButton2.Checked;
            UseRmcKey = false;
        }
    }
}
