using System;
using System.Windows.Forms;
using QuazalWV;

namespace AcbRdv
{
    public partial class SendNotification : Form
    {
        public SendNotification()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (ClientInfo client in Global.Clients)
                {
                    NotificationQueue.AddNotification(
                        new NotificationEvent(client, 
                            0,
                            Convert.ToUInt32(textBox1.Text),
                            Convert.ToUInt32(textBox2.Text),
                            Convert.ToUInt32(textBox3.Text),
                            Convert.ToUInt32(textBox4.Text),
                            Convert.ToUInt32(textBox5.Text),
                            Convert.ToUInt32(textBox6.Text),
                            textBox7.Text));
                }
            }
            catch { }
        }
    }
}
