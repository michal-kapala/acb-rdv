using System.IO;

namespace QuazalWV
{
    public class QBuffer : IData
    {
        public ushort Length { get; set; }
        public byte[] Data { get; set; }

        public QBuffer()
        {

        }

        public QBuffer(byte[] data)
        {
            Length = (ushort)data.Length;
            Data = data;
        }

        public QBuffer(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Length = Helper.ReadU16(s);
            Data = new byte[Length];
            for (int i = 0; i < Length; i++)
            Data[i] = (byte)s.ReadByte();
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU16(s, Length);
            for (int i = 0; i < Length; i++)
            Helper.WriteU8(s, Data[i]);
        }
    }
}
