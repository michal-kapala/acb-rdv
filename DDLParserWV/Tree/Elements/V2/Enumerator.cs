using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace DDLParserWV
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Enumerator : ParseTreeItem<Enumerator>
    {
        public override EParseTreeElement Type { get; set; } = EParseTreeElement.Enumerator;
        [JsonProperty("nsItem")]
        public NameSpaceItem NsItem { get; set; } = new NameSpaceItem();
        [JsonProperty("type")]
        public string Value { get; set; }

        protected override Enumerator ParseTyped(Stream s, StringBuilder log, uint depth, uint majorVersion)
        {
            string tabs = Utils.MakeTabs(depth);
            log.AppendLine($"{tabs}[EnumDeclaration]");
            NsItem.Parse(s, log, depth + 1, majorVersion);
            Value = Utils.ReadString(s);
            log.AppendLine($"{tabs}\t[Value: {Value}]");
            return this;
        }
    }
}
