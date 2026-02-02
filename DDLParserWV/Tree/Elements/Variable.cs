using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace DDLParserWV
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Variable : ParseTreeItem<Variable>
    {
        public override EParseTreeElement Type { get; set; } = EParseTreeElement.Variable;
        [JsonProperty("nsItem")]
        public NameSpaceItem NsItem { get; set; } = new NameSpaceItem();
        [JsonProperty("declUse")]
        public DeclarationUse DeclarationUse { get; set; }
        [JsonProperty("arraySize")]
        public uint ArraySize { get; set; }
        [JsonProperty("unknown_v2")]
        public uint V2_Unk { get; set; }
        private uint Version { get; set; }

        protected override Variable ParseTyped(Stream s, StringBuilder log, uint depth, uint majorVersion)
        {
            Version = majorVersion;
            string tabs = Utils.MakeTabs(depth);
            log.AppendLine($"{tabs}[Variable]");
            NsItem.Parse(s, log, depth + 1, Version);
            byte declUseType = (byte)s.ReadByte();
            DeclarationUse = new DeclarationUse(s, (EParseTreeElement)declUseType, log, depth + 1, Version);
            ArraySize = Utils.ReadU32(s);
            log.AppendLine($"{tabs}\t[arraySize: {ArraySize}]");
            // only present in v2
            if (Version == 2)
            {
                V2_Unk = Utils.ReadU32(s);
                log.AppendLine($"{tabs}\t[V2_Unk: {V2_Unk}]");
            }
            return this;
        }

        public string GetName()
        {
            return NsItem.NsItemName;
        }

        public string GetFullType()
        {
            return DeclarationUse.TypeName;
        }

        public bool ShouldSerializeV2_Unk()
        {
            return Version == 2;
        }
    }
}
