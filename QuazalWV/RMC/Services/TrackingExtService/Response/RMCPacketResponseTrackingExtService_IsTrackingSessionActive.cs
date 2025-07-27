using System;
using System.IO;

namespace QuazalWV
{
    public class RMCPacketResponseTrackingExtService_IsTrackingSessionActive : RMCPResponse
    {
        public bool IsActive { get; set; }

        public RMCPacketResponseTrackingExtService_IsTrackingSessionActive()
        {
            IsActive = true;
        }

        public override string ToString()
        {
            return "[IsTrackingSessionActive Response]";
        }

        public override string PayloadToString()
        {
            return $"\t[IsActive: {IsActive}]";
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteBool(m, IsActive);
            return m.ToArray();
        }
    }
}
