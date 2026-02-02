using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace DDLParserWV
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DDLUnitDeclaration : ParseTreeItem<DDLUnitDeclaration>
    {
        public override EParseTreeElement Type { get; set; } = EParseTreeElement.DDLUnitDeclaration;
        [JsonProperty("decl")]
        public Declaration Declaration { get; set; } = new Declaration();
        [JsonProperty("unitName")]
        public string UnitName { get; set; }
        [JsonProperty("unitDir")]
        public string UnitDir { get; set; }
        [JsonProperty("unknown_v2")]
        public byte V2_Unk {  get; set; }
        private uint Version { get; set; }

        protected override DDLUnitDeclaration ParseTyped(Stream s, StringBuilder log, uint depth, uint majorVersion)
        {
            Version = majorVersion;
            string tabs = Utils.MakeTabs(depth);
            log.AppendLine($"{tabs}[DDLUnitDeclaration]");
            Declaration.Parse(s, log, depth + 1, Version);
            UnitName = Utils.ReadString(s);
            log.AppendLine($"{tabs}\t[unitName: {UnitName}]");
            UnitDir = Utils.ReadString(s);
            log.AppendLine($"{tabs}\t[unitDir: {UnitDir}]");
            // only present in v2
            if (Version == 2)
            {
                V2_Unk = (byte)s.ReadByte();
                log.AppendLine($"{tabs}\t[V2_Unk: {V2_Unk}]");
            }
            return this;
        }

        public bool ShouldSerializeV2_Unk()
        {
            return Version == 2;
        }
    }
}
