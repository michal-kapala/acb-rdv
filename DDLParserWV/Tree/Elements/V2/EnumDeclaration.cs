using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DDLParserWV
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EnumDeclaration : ParseTreeItem<EnumDeclaration>
    {
        public override EParseTreeElement Type { get; set; } = EParseTreeElement.EnumDeclaration;
        [JsonProperty("decl")]
        public Declaration Declaration { get; set; } = new Declaration();
        [JsonProperty("versions")]
        public uint VersionsSize { get; set; }
        public List<EnumDeclarationVersion> Versions { get; set; } = new List<EnumDeclarationVersion>();

        protected override EnumDeclaration ParseTyped(Stream s, StringBuilder log, uint depth, uint majorVersion)
        {
            string tabs = Utils.MakeTabs(depth);
            log.AppendLine($"{tabs}[EnumDeclaration]");
            Declaration.Parse(s, log, depth + 1, majorVersion);
            VersionsSize = Utils.ReadU32(s);
            log.AppendLine($"{tabs}\t[Versions.Size: {VersionsSize}]");
            for (uint i = 0; i < VersionsSize; i++)
                Versions.Add(new EnumDeclarationVersion(s, log, depth + 1, majorVersion));
            return this;
        }
    }
}
