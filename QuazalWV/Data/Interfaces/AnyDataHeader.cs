using System.IO;

namespace QuazalWV
{
    public class AnyDataHeader : IData
    {
        public string DataClass { get; set; }
        public uint OuterSize { get; set; }
        public uint InnerSize { get; set; }

        public AnyDataHeader(string name, uint innerSize)
        {
            DataClass = name;
            OuterSize = innerSize + 4;
            InnerSize = innerSize;
        }

        public AnyDataHeader(Stream s)
        {
            FromStream(s);
        }

        public virtual void FromStream(Stream s)
        {
            DataClass = Helper.ReadString(s);
            OuterSize = Helper.ReadU32(s);
            InnerSize = Helper.ReadU32(s);
        }

        public virtual void ToBuffer(Stream s)
        {
            Helper.WriteString(s, DataClass);
            Helper.WriteU32(s, OuterSize);
            Helper.WriteU32(s, InnerSize);
        }
    }
}
