using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketResponseNewsService_GetChannelsByTypes : RMCPResponse
    {
        public List<NewsChannel> Channels { get; set; }

        public RMCPacketResponseNewsService_GetChannelsByTypes(ClientInfo client)
        {
            NewsChannel soloChannel = new NewsChannel("Solo", client.LocaleCode);
            soloChannel.Gath.m_pidHost = client.User.Pid;
            Channels = new List<NewsChannel>{ soloChannel };
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Channels: {Channels.Count}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)Channels.Count);
            foreach (NewsChannel channel in Channels)
                channel.ToBuffer(m);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[GetChannelsByTypes Response]";
        }
    }
}
