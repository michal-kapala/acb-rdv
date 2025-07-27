using System.IO;

namespace QuazalWV
{
    public class ResultRange : IData
    {
        public uint Offset { get; set; }
        public uint Size { get; set; }

        public ResultRange(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Offset = Helper.ReadU32(s);
            Size = Helper.ReadU32(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU32(s, Offset);
            Helper.WriteU32(s, Size);
        }
    }
}
