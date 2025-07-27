using System.IO;

namespace QuazalWV
{
    public class Privilege : IData
    {
        public uint Id { get; set; }
        public string Description { get; set; }

        public Privilege()
        {
            Id = 1;
            Description = "Allow to play online";
        }

        public Privilege(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Id = Helper.ReadU32(s);
            Description = Helper.ReadString(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU32(s, Id);
            Helper.WriteString(s, Description);
        }
    }
}
