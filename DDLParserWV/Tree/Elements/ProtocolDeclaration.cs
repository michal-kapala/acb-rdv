using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace DDLParserWV
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ProtocolDeclaration : ParseTreeItem<ProtocolDeclaration>
    {
        public override EParseTreeElement Type { get; set; } = EParseTreeElement.ProtocolDeclaration;
        [JsonProperty("decl")]
        public Declaration Declaration { get; set; } = new Declaration();
        [JsonProperty("namespace")]
        public NameSpace NameSpace { get; set; }

        protected override ProtocolDeclaration ParseTyped(Stream s, StringBuilder log, uint depth, uint majorVersion)
        {
            string tabs = Utils.MakeTabs(depth);
            log.AppendLine($"{tabs}[ProtocolDeclaration]");
            Declaration.Parse(s, log, depth + 1, majorVersion);
            NameSpace = new NameSpace(s, log, depth + 1, majorVersion);
            return this;
        }
    }
}
