using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuazalWV;
using Be.Windows.Forms;

namespace AcbRdv
{
    public partial class PacketGenerator : Form
    {
        public List<string> protoNames = new List<string>();
        public List<int> protoIDs = new List<int>();

        public PacketGenerator()
        {
            InitializeComponent();
        }

        private void PacketGenerator_Load(object sender, EventArgs e)
        {
            protoNames.AddRange(Enum.GetNames(typeof(RMCP.PROTOCOL)));
            protoIDs.AddRange(Enum.GetValues(typeof(RMCP.PROTOCOL)).Cast<int>());
            while (true)
            {
                bool found = false;
                for (int i = 0; i < protoNames.Count - 1; i++)
                {
                    if (protoNames[i].CompareTo(protoNames[i + 1]) > 0)
                    {
                        found = true;
                        string tmp = protoNames[i];
                        protoNames[i] = protoNames[i + 1];
                        protoNames[i + 1] = tmp;
                        int tmp2 = protoIDs[i];
                        protoIDs[i] = protoIDs[i + 1];
                        protoIDs[i + 1] = tmp2;
                    }
                }
                if (!found)
                    break;
            }
            toolStripComboBox1.Items.Clear();
            for (int i = 0; i < protoNames.Count; i++)
                toolStripComboBox1.Items.Add(protoIDs[i].ToString("X2") + " - " + protoNames[i]);
            toolStripComboBox1.SelectedIndex = 0;
            hb1.ByteProvider = new DynamicByteProvider(new byte[4]);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            MemoryStream m = new MemoryStream();
            for (long i = 0; i < hb1.ByteProvider.Length; i++)
                m.WriteByte(hb1.ByteProvider.ReadByte(i));
            byte[] payload = m.ToArray();
            foreach (ClientInfo client in Global.Clients)
            {
                PrudpPacket q = new PrudpPacket
                {
                    m_oSourceVPort = new PrudpPacket.VPort(0x31),
                    m_oDestinationVPort = new PrudpPacket.VPort(0x3f),
                    type = PrudpPacket.PACKETTYPE.DATA,
                    flags = new List<PrudpPacket.PACKETFLAG>(),
                    payload = new byte[0],
                    uiSeqId = (ushort)(++client.gameSeqId),
                    m_bySessionID = client.sessionID
                };
                RMCP rmc = new RMCP
                {
                    proto = (RMCP.PROTOCOL)protoIDs[toolStripComboBox1.SelectedIndex],
                    methodID = Convert.ToUInt32(toolStripTextBox1.Text),
                    callID = ++client.callCounterRMC
                };
                RMCPCustom reply = new RMCPCustom
                {
                    buffer = payload
                };
                RMC.SendRequestPacket(q, rmc, client, reply, true, 0);
            }
        }
    }
}
