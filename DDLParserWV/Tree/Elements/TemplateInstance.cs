using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace DDLParserWV
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TemplateInstance : ParseTreeItem<TemplateInstance>
    {
        public override EParseTreeElement Type { get; set; } = EParseTreeElement.TemplateInstance;
        [JsonProperty("decl")]
        public Declaration Declaration { get; set; } = new Declaration();
        [JsonProperty("templateType")]
        public TemplateType TemplateType { get; set; } = new TemplateType();

        protected override TemplateInstance ParseTyped(Stream s, StringBuilder log, uint depth, uint majorVersion)
        {
            string tabs = Utils.MakeTabs(depth);
            log.AppendLine($"{tabs}[TemplateInstance]");
            Declaration.Parse(s, log, depth + 1, majorVersion);
            TemplateType.Parse(s, log, depth + 1, majorVersion);
            return this;
        }
    }
}
