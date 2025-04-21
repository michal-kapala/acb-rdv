using System;
using System.IO;

namespace QuazalWV
{
    internal class RMCPacketRequestFriendsService_DeclineFriendship : RMCPRequest
    {
        public uint Pid { get; set; }
        public uint Details { get; set; }
        public static void PrintEntireStreamAsHex(Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("Stream must support seeking.");
            }

            long originalPosition = stream.Position;

            // Read entire stream
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                byte[] data = ms.ToArray();
                string build = "";
                // Print bytes in hex format
                foreach (byte b in data)
                {
                    build += $"{b:X2} ";
                }

                Log.WriteLine(1, build);

                // Reset original stream position
                stream.Position = originalPosition;
            }
        }
        public RMCPacketRequestFriendsService_DeclineFriendship(Stream s)
        {
            PrintEntireStreamAsHex(s);
            Pid = Helper.ReadU32(s);

        }

        public override string ToString()
        {
            return "[BlackList Request]";
        }

        public override string PayloadToString()
        {
            return "";
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, Pid);
            Helper.WriteU32(m, Details);
            return m.ToArray();
        }
    }

}