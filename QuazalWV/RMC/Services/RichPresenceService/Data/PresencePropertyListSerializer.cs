using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace QuazalWV
{
    public class PresencePropertyListSerializer
    {
        private List<PresenceProperty> Props { get; set; }

        public PresencePropertyListSerializer(List<PresenceProperty> props)
        {
            Props = props;
        }

        public byte[] Serialize()
        {
            uint size = (uint)Props.Count;
            if (size == 0)
                return new byte[8] { 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30 };
            string result = U32ToHexLE(size);
            foreach (var prop in Props)
                result += SerializeProp(prop);
            return Encoding.ASCII.GetBytes(result);
        }

        private string SerializeProp(PresenceProperty prop)
        {
            string result = U32ToHexLE((uint)prop.Id);
            result += U32ToHexLE((uint)prop.DataType);
            switch(prop.DataType)
            {
                case VariantType.Int32:
                    result += U32ToHexLE(prop.Value);
                    break;
                default:
                    Log.WriteLine(1, $"{prop.DataType} variant type unsupported", LogSource.PresenceSerializer, Color.Red);
                    return "";
            }
            return result;
        }

        private string U32ToHexLE(uint nb)
        {
            byte lsb = (byte)(nb & 0xff);
            byte second = (byte)((nb >> 8) & 0xff);
            byte third = (byte)((nb >> 16) & 0xff);
            byte msb = (byte)((nb >> 24) & 0xff);
            return $"{lsb:X2}{second:X2}{third:X2}{msb:X2}";
        }
    }
}
