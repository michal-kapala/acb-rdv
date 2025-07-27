using System.IO;

namespace QuazalWV
{
    public class QFile : IData
    {
        public string Name { get; set; }
        public uint Size { get; set; }

        public QFile(string name)
        {
            Name = name;
            // Dumped from the original traffic, could be a key/ID or size in bytes.
            Size = 58484;
        }

        public QFile(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Name = Helper.ReadString(s);
            Size = Helper.ReadU32(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteString(s, Name);
            Helper.WriteU32(s, Size);
        }
    }
}
