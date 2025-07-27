namespace QuazalWV
{
    public class RMCPCustom : RMCPResponse
    {
        public byte[] buffer;

        public override byte[] ToBuffer()
        {
            return buffer;
        }

        public override string ToString()
        {
            return "[RMCPacketCustom]";
        }

        public override string PayloadToString()
        {
            return "";
        }
    }
}
