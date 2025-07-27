using System.IO;

namespace QuazalWV
{
    public class NewsChannel : IData
    {
        public Gathering Gath { get; set; }
        public string Name { get; set; }
        public string ChannelType { get; set; }
        public string Locale { get; set; }
        public bool Subscribable { get; set; }

        public NewsChannel(string name, string locale = "en-US")
        {
            Gath = new Gathering()
            {
                m_uiMaxParticipants = 64,
                m_uiParticipationPolicy = (uint)PARTICIPATION_POLICY.NewsChannelDefault,
                m_strDescription = name
            };
            Name = name;
            ChannelType = name;
            Locale = locale;
            Subscribable = false;
        }

        public NewsChannel(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Gath = new Gathering(s);
            Name = Helper.ReadString(s);
            ChannelType = Helper.ReadString(s);
            Locale = Helper.ReadString(s);
            Subscribable = Helper.ReadBool(s);
        }

        public void ToBuffer(Stream s)
        {
            Gath.ToBuffer(s);
            Helper.WriteString(s, Name);
            Helper.WriteString(s, ChannelType);
            Helper.WriteString(s, Locale);
            Helper.WriteBool(s, Subscribable);
        }
    }
}
