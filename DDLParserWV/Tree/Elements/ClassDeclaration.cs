using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace DDLParserWV
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ClassDeclaration : ParseTreeItem<ClassDeclaration>
    {
        public override EParseTreeElement Type { get; set; } = EParseTreeElement.ClassDeclaration;
        [JsonProperty("typeDecl")]
        public Declaration TypeDeclaration { get; set; } = new Declaration();
        [JsonProperty("unknown1_v2")]
        public byte V2_Unk1 { get; set; }
        [JsonProperty("parentNamespace")]
        public string ParentNamespaceName { get; set; }
        [JsonProperty("unknown2_v2")]
        public uint V2_Unk2 { get; set; }
        [JsonProperty("unknown3_v2")]
        public uint V2_Unk3 { get; set; }
        [JsonProperty("namespace")]
        public NameSpace NameSpace { get; set; }
        private uint Version { get; set; }

        protected override ClassDeclaration ParseTyped(Stream s, StringBuilder log, uint depth, uint majorVersion)
        {
            Version = majorVersion;
            string tabs = Utils.MakeTabs(depth);
            log.AppendLine($"{tabs}[ClassDeclaration]");
            TypeDeclaration.Parse(s, log, depth + 1, Version);
            // present only in v2
            if (Version == 2)
            {
                V2_Unk1 = (byte)s.ReadByte();
                log.AppendLine($"{tabs}\t[V2_Unk1: {V2_Unk1}]");
            }
            ParentNamespaceName = Utils.ReadString(s);
            log.AppendLine($"{tabs}\t[parentNamespace: {ParentNamespaceName}]");
            // present only in v2
            if (Version == 2)
            {
                V2_Unk2 = Utils.ReadU32(s);
                log.AppendLine($"{tabs}\t[V2_Unk2: {V2_Unk2}]");
                V2_Unk3 = Utils.ReadU32(s);
                log.AppendLine($"{tabs}\t[V2_Unk3: {V2_Unk3}]");
            }
            NameSpace = new NameSpace(s, log, depth + 1, Version);
            return this;
        }

        public string GetName()
        {
            return TypeDeclaration.NsItem.NsItemName;
        }

        public bool ShouldSerializeV2_Unk1()
        {
            return Version == 2;
        }

        public bool ShouldSerializeV2_Unk2()
        {
            return Version == 2;
        }

        public bool ShouldSerializeV2_Unk3()
        {
            return Version == 2;
        }
    }
}
