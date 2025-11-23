using Ionic.Zlib;
using System.Text;  
using System.Security.Cryptography;

namespace QuazalWV
{
    public static class Helper
    {
        public static ulong MakeTimestamp()
        {
            // Use current UTC time so NAT pings get unique timestamps.
            return (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public static bool ReadBool(Stream s)
        {
            return s.ReadByte() != 0;
        }
        public static byte ReadU8(Stream s)
        {
            return (byte)s.ReadByte();
        }

        public static ushort ReadU16(Stream s)
        {
            return (ushort)((byte)s.ReadByte() | ((byte)s.ReadByte() << 8));
        }

        public static ushort ReadU16LE(Stream s)
        {
            return (ushort)(((byte)s.ReadByte() << 8) | (byte)s.ReadByte());
        }

        public static uint ReadU32(Stream s)
        {
            BinaryReader reader = new(s);
            return reader.ReadUInt32();
        }

        public static ulong ReadU64(Stream s)
        {
            return (ulong)((byte)s.ReadByte() | 
                          ((byte)s.ReadByte() << 8) | 
                          ((byte)s.ReadByte() << 16) |
                          ((byte)s.ReadByte() << 24) |
                          ((byte)s.ReadByte() << 32) |
                          ((byte)s.ReadByte() << 40) |
                          ((byte)s.ReadByte() << 48) |
                          ((byte)s.ReadByte() << 56));
        }

        public static ulong ReadU64DateTime(Stream s)
        {
            BinaryReader reader = new(s);
            return reader.ReadUInt64();
        }

        public static float ReadFloat(Stream s)
        {
            byte[] b = new byte[4];
            s.ReadExactly(b, 0, 4);
            return BitConverter.ToSingle(b, 0);
        }

        public static double ReadDouble(Stream s)
        {
            byte[] b = new byte[8];
            s.ReadExactly(b, 0, 8);
            return BitConverter.ToDouble(b, 0);
        }

        public static string ReadString(Stream s)
        {
            string result = "";
            ushort len = ReadU16(s);
            for (int i = 0; i < len - 1; i++)
                result += (char)s.ReadByte();
            s.ReadByte();
            return result;
        }

        public static List<string> ReadStringList(Stream s)
        {
            uint count = ReadU32(s);
            List<string> list = [];
            for (int i = 0; i < count; i++)
                list.Add(ReadString(s));
            return list;
        }

        public static void WriteU8(Stream s, byte v)
        {
            s.WriteByte(v);
        }

        public static void WriteBool(Stream s, bool v)
        {
            s.WriteByte((byte)(v ? 1 : 0));
        }

        public static void WriteU16(Stream s, ushort v)
        {
            s.WriteByte((byte)v);
            s.WriteByte((byte)(v >> 8));
        }

        public static void WriteU32(Stream s, uint v)
        {
            s.WriteByte((byte)v);
            s.WriteByte((byte)(v >> 8));
            s.WriteByte((byte)(v >> 16));
            s.WriteByte((byte)(v >> 24));
        }

        public static void WriteU16LE(Stream s, ushort v)
        {
            s.WriteByte((byte)(v >> 8));
            s.WriteByte((byte)v);
        }

        public static void WriteU32LE(Stream s, uint v)
        {
            s.WriteByte((byte)(v >> 24));
            s.WriteByte((byte)(v >> 16));
            s.WriteByte((byte)(v >> 8));
            s.WriteByte((byte)v);
        }

        public static void WriteU64(Stream s, ulong v)
        {
            s.WriteByte((byte)v);
            s.WriteByte((byte)(v >> 8));
            s.WriteByte((byte)(v >> 16));
            s.WriteByte((byte)(v >> 24));
            s.WriteByte((byte)(v >> 32));
            s.WriteByte((byte)(v >> 40));
            s.WriteByte((byte)(v >> 48));
            s.WriteByte((byte)(v >> 56));
        }

        public static void WriteFloat(Stream s, float v)
        {
            byte[] b = BitConverter.GetBytes(v);
            s.Write(b, 0, 4);
        }

        public static void WriteFloatLE(Stream s, float v)
        {
            byte[] b = BitConverter.GetBytes(v);
            s.WriteByte(b[3]);
            s.WriteByte(b[2]);
            s.WriteByte(b[1]);
            s.WriteByte(b[0]);
        }

        public static void WriteDouble(Stream s, double v)
        {
            byte[] b = BitConverter.GetBytes(v);
            s.Write(b, 0, 8);
        }

        public static void WriteString(Stream s, string v)
        {
            if (v != null)
            {
                WriteU16(s, (ushort)(v.Length + 1));
                foreach (char c in v)
                    s.WriteByte((byte)c);
                s.WriteByte(0);
            }
            else
            {
                s.WriteByte(1);
                s.WriteByte(0);
                s.WriteByte(0);
            }
        }

        public static void WriteStringList(Stream s, List<string> v)
        {
            WriteU32(s, (uint)v.Count);
            foreach(var entry in v)
                WriteString(s, entry);
        }

        public static byte[] Decompress(byte[] data)
        {
            using ZlibStream s = new(new MemoryStream(data), CompressionMode.Decompress);
            using MemoryStream result = new();
            s.CopyTo(result);
            return result.ToArray();
        }

        public static byte[] Compress(byte[] data)
        {
            using ZlibStream s = new(new MemoryStream(data), CompressionMode.Compress);
            using MemoryStream result = new();
            s.CopyTo(result);
            return result.ToArray();
        }

        public static byte[] Encrypt(string key, byte[] data)
        {
            return Encrypt(Encoding.ASCII.GetBytes(key), data);
        }

        public static byte[] Decrypt(string key, byte[] data)
        {
            return Encrypt(Encoding.ASCII.GetBytes(key), data);
        }

        public static byte[] Encrypt(byte[] key, byte[] data)
        {
            return [.. EncryptOutput(key, data)];
        }

        public static byte[] Decrypt(byte[] key, byte[] data)
        {
            return [.. EncryptOutput(key, data)];
        }

        private static byte[] EncryptInitalize(byte[] key)
        {
            byte[] s = [.. Enumerable.Range(0, 256).Select(i => (byte)i)];
            for (int i = 0, j = 0; i < 256; i++)
            {
                j = (j + key[i % key.Length] + s[i]) & 255;

                Swap(s, i, j);
            }
            return s;
        }

        private static IEnumerable<byte> EncryptOutput(byte[] key, IEnumerable<byte> data)
        {
            byte[] s = EncryptInitalize(key);
            int i = 0;
            int j = 0;
            return data.Select((b) =>
            {
                i = (i + 1) & 255;
                j = (j + s[i]) & 255;
                Swap(s, i, j);
                return (byte)(b ^ s[(s[i] + s[j]) & 255]);
            });
        }

        private static void Swap(byte[] s, int i, int j)
        {
            (s[j], s[i]) = (s[i], s[j]);
        }

        public static byte[] DeriveKey(uint pid, string input = "UbiDummyPwd")
        {
            uint count = 65000 + (pid % 1024);
            byte[] buff = Encoding.ASCII.GetBytes(input);
            for (uint i = 0; i < count; i++)
                buff = MD5.HashData(buff);
            return buff;
        }

        /// <summary>
        /// RC4 key dumped from ACB.
        /// </summary>
        /// <returns></returns>
        public static byte[] P2pKey()
        {
            return
            [
                0x37, 0x1E, 0x29, 0xAD, 0xFA, 0xAB, 0xF0, 0x8D, 0xCA, 0xBA, 0x9D, 0xD8, 0x63, 0xBC, 0x0A, 0x8E,
                0x79, 0x5E, 0xC7, 0xBB, 0x9D, 0x90, 0x05, 0x9C, 0xDA, 0x8F, 0x82, 0xCD, 0xFE, 0x55, 0xCC, 0xDC,
                0x0F, 0xBC, 0xA0, 0x8F, 0x4F, 0x9B, 0x67, 0x9D, 0xDE, 0x9E, 0x90, 0xCE, 0xF9, 0xAF, 0xFA, 0xFD
            ];
        }

        public static byte[] MakeHMAC(byte[] key, byte[] data)
        {
            return HMACMD5.HashData(key, data);
        }

        public static byte[] MakeFilledArray(int len)
        {
            byte[] result = new byte[len];
            for (int i = 0; i < len; i++)
                result[i] = (byte)i;
            return result;
        }
    }
}
