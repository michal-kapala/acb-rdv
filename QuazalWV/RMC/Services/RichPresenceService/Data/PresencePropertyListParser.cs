using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace QuazalWV
{
    public class PresencePropertyListParser
    {
        private string Buffer { get; set; }

        public PresencePropertyListParser(string buffer)
        {
            Buffer = buffer;
        }

        public List<PresenceProperty> Parse()
        {
            var props = new List<PresenceProperty>();
            try
            {
                uint size = ReadWordLE();
                for (uint i = 0; i < size; i++)
                    props.Add(ParseProperty());
            }
            catch (Exception ex)
            {
                Log.WriteLine(1, $"{ex}", LogSource.PresenceParser, Color.Red);
            }
            return props;
        }

        private PresenceProperty ParseProperty()
        {
            var id = (PresencePropertyId)ReadWordLE();
            var type = (VariantType)ReadWordLE();
            uint value;
            switch (type)
            {
                case VariantType.Int32:
                    value = ReadWordLE();
                    break;
                default:
                    throw new Exception($"{type} variant type unsupported");
            }

            return new PresenceProperty
            {
                Id = id,
                DataType = type,
                Value = value
            };
        }

        private uint ReadWordLE()
        {
            uint result = 0;
            if (Buffer.Length < 8 || Buffer.Length % 8 != 0)
                Log.WriteLine(1, $"Invalid buffer length {Buffer.Length}", LogSource.PresenceParser, Color.Red);

            result += ReadByte();
            result += (uint)(ReadByte()) << 8;
            result += (uint)(ReadByte()) << 16;
            result += (uint)(ReadByte()) << 24;
            return result;
        }

        private byte ReadByte()
        {
            string hex = Buffer.Substring(0, 2);
            Buffer = Buffer.Substring(2);
            return byte.Parse(hex, NumberStyles.HexNumber);
        }
    }
}
