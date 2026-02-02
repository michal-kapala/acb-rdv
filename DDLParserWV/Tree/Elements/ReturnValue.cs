using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace DDLParserWV
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ReturnValue : ParseTreeItem<ReturnValue>
    {
        public override EParseTreeElement Type { get; set; } = EParseTreeElement.ReturnValue;
        [JsonProperty("var")]
        public Variable Variable { get; set; } = new Variable();
        [JsonProperty("declUse")]
        public DeclarationUse DeclarationUse { get; set; }
        [JsonProperty("arraySize")]
        public uint ArraySize { get; set; }
        [JsonProperty("unknown_v2")]
        public uint V2_Unk {  get; set; }
        private uint Version { get; set; }

        protected override ReturnValue ParseTyped(Stream s, StringBuilder log, uint depth, uint majorVersion)
        {
            Version = majorVersion;
            string tabs = Utils.MakeTabs(depth);
            log.AppendLine($"{tabs}[ReturnValue]");
            Variable.Parse(s, log, depth + 1, Version);
            byte type = (byte)s.ReadByte();
            DeclarationUse = new DeclarationUse(s, (EParseTreeElement)type, log, depth + 1, Version);
            ArraySize = Utils.ReadU32(s);
            log.AppendLine($"{tabs}\t[arraySize: {ArraySize}]");
            if (Version == 2)
            {
                V2_Unk = Utils.ReadU32(s);
                log.AppendLine($"{tabs}\t[V2_Unk: {V2_Unk}]");
            }
            return this;
        }

        public string GetFullType()
        {
            return DeclarationUse.TypeName;
        }

        public string GetName()
        {
            return Variable.NsItem.NsItemName;
        }

        public bool ShouldSerializeV2_Unk()
        {
            return Version == 2;
        }
    }
}
