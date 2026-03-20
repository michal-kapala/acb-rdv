using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace DDLParserWV
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NameSpaceItem : ParseTreeItem<NameSpaceItem>
    {
        public override EParseTreeElement Type { get; set; } = EParseTreeElement.NameSpaceItem;
        [JsonProperty("treeItemName")]
        public string TreeItemName { get; set; }
        [JsonProperty("unknown1_v2")]
        public uint V2_Unk1 { get; set; }
        [JsonProperty("nsItemName")]
        public string NsItemName { get; set; }
        [JsonProperty("unknown2_v2")]
        public uint V2_Unk2 { get; set; }
        private uint Version { get; set; }

        protected override NameSpaceItem ParseTyped(Stream s, StringBuilder log, uint depth, uint majorVersion)
        {
            Version = majorVersion;
            string tabs = Utils.MakeTabs(depth);
            log.AppendLine($"{tabs}[NameSpaceItem]");
            TreeItemName = Utils.ReadString(s);
            log.AppendLine($"{tabs}\t[TreeItemName: {TreeItemName}]");
            // only present in v2
            if (Version == 2)
            {
                V2_Unk1 = Utils.ReadU32(s);
                log.AppendLine($"{tabs}\t[V2_Unk1: {V2_Unk1}]");
            }
            NsItemName = Utils.ReadString(s);
            log.AppendLine($"{tabs}\t[NsItemName: {NsItemName}]");
            // only present in v2
            if (Version == 2)
            {
                V2_Unk2 = Utils.ReadU32(s);
                log.AppendLine($"{tabs}\t[V2_Unk2: {V2_Unk2}]");
            }
            return this;
        }

        public bool ShouldSerializeV2_Unk1()
        {
            return Version == 2;
        }

        public bool ShouldSerializeV2_Unk2()
        {
            return Version == 2;
        }
    }
}
