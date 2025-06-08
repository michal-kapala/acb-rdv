using System.IO;

namespace QuazalWV
{
    public class PresenceElement : IData
    {
        public uint Pid { get; set; }
        public bool OverrideStatus {  get; set; } // IsPlaying?
        public uint UnkInt2 { get; set; } // SessionId?
        public QBuffer PropsBuffer { get; set; }

        public PresenceElement()
        {
            PropsBuffer = new QBuffer();
        }

        public PresenceElement(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Pid = Helper.ReadU32(s);
            OverrideStatus = Helper.ReadBool(s);
            UnkInt2 = Helper.ReadU32(s);
            PropsBuffer = new QBuffer(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU32(s, Pid);
            Helper.WriteBool(s, OverrideStatus);
            Helper.WriteU32(s, UnkInt2);
            PropsBuffer.ToBuffer(s);
        }
    }
}
