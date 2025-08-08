using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public class PresenceProperty : IData
    {
        public PresencePropertyId Id { get; set; }
        public VariantType DataType { get; set; }
        /// <summary>
        /// ACB always uses int32 variants.
        /// </summary>
        public uint Value { get; set; }

        public PresenceProperty()
        {

        }

        public PresenceProperty(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Id = (PresencePropertyId)Helper.ReadU32(s);
            DataType = (VariantType)Helper.ReadU32(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU32(s, (uint)Id);
            Helper.WriteU32(s, (uint)DataType);
            switch (DataType)
            {
                case VariantType.Int32:
                    Helper.WriteU32(s, Value);
                    break;
                default:
                    Log.WriteLine(1, $"{DataType} variant type unsupported", LogSource.PresenceProperty, Color.Red);
                    break;
            }
        }

        public override string ToString()
        {
            switch (Id) {
                case PresencePropertyId.IsInSession:
                    string value = Value == 10 ? "yes" : "no";
                    return $"{Id}: {value}";
                case PresencePropertyId.Map:
                    return $"{Id}: {(Map)Value}";
                case PresencePropertyId.GameMode:
                    return $"{Id}: {(GameMode)Value}";
                case PresencePropertyId.GameType:
                    return $"{Id}: {(GameType)Value}";
                case PresencePropertyId.SessionDLCMask:
                    return $"{Id}: {Value}";
                default:
                    Log.WriteLine(1, $"Unknown presence property {(uint)Id}", LogSource.PresenceProperty, Color.Red);
                    return $"{(uint)Id}: {Value}";
            }
        }
    }
}
