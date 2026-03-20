using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace DDLParserWV
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EnumDeclarationVersion
    {
        [JsonProperty("version")]
        public uint Version { get; set; }
        [JsonProperty("namespace")]
        public NameSpace NameSpace { get; set; }

        public EnumDeclarationVersion(Stream s, StringBuilder log, uint depth, uint majorVersion)
        {
            string tabs = Utils.MakeTabs(depth);
            log.AppendLine($"{tabs}[EnumDeclarationVersion]");
            Version = Utils.ReadU32(s);
            log.AppendLine($"{tabs}\t[Version: {Version}]");
            NameSpace = new NameSpace(s, log, depth + 1, majorVersion);
        }
    }
}
