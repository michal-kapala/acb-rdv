using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

namespace QuazalWV
{
    public class RMCPacketResponsePersistentStoreService_GetItem : RMCPResponse
    {
        public byte[] Content { get; set; }
        public bool Result { get; set; }

        public RMCPacketResponsePersistentStoreService_GetItem(string fileName)
        {
            try
            {
                Content = File.ReadAllBytes(fileName);
            }
            catch
            {
                Content = new byte[0];
                Log.WriteLine(1, $"[RMC Persistent Store] Cannot read {fileName}", Color.Red);
            }
            Result = true;
        }

        public override string ToString()
        {
            return "[GetItem Response]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[File size: {Content.Length}]");
            sb.AppendLine($"\t[Result: {Result}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)Content.Length);
            m.Write(Content, 0, Content.Length);
            Helper.WriteBool(m, Result);
            return m.ToArray();
        }
    }
}
