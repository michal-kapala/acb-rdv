using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DDLParserWV
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MethodDeclaration : ParseTreeItem<MethodDeclaration>
    {
        public override EParseTreeElement Type { get; set; } = EParseTreeElement.MethodDeclaration;
        [JsonProperty("decl")]
        public Declaration Declaration { get; set; } = new Declaration();
        [JsonProperty("variantCount1")]
        public uint V2_RequestVariantCount1 { get; set; }
        [JsonProperty("variants1")]
        public List<MethodVariant> V2_Variants1 { get; set; }
        [JsonProperty("variantCount2")]
        public uint V2_RequestVariantCount2 { get; set; }
        [JsonProperty("variants2")]
        public List<MethodVariant> V2_Variants2 { get; set; }
        [JsonProperty("namespace")]
        public NameSpace NameSpace { get; set; } = new NameSpace();
        private uint Version { get; set; }

        protected override MethodDeclaration ParseTyped(Stream s, StringBuilder log, uint depth, uint majorVersion)
        {
            Version = majorVersion;
            string tabs = Utils.MakeTabs(depth);
            log.AppendLine($"{tabs}[MethodDeclaration]");
            Declaration.Parse(s, log, depth + 1, Version);
            // only present in v1
            if (Version == 1)
                NameSpace = new NameSpace(s, log, depth + 1, Version);
            // only present in v2
            else if (Version == 2)
            {
                V2_Variants1 = new List<MethodVariant>();
                V2_Variants2 = new List<MethodVariant>();
                // interpret as 2 lists
                V2_RequestVariantCount1 = Utils.ReadU32(s);
                log.AppendLine($"{tabs}\t[V2_RequestVariantCount1: {V2_RequestVariantCount1}]");
                for (uint i = 0; i < V2_RequestVariantCount1; i++)
                    V2_Variants1.Add(new MethodVariant(s, log, depth + 1, Version));

                V2_RequestVariantCount2 = Utils.ReadU32(s);
                log.AppendLine($"{tabs}\t[V2_RequestVariantCount2: {V2_RequestVariantCount2}]");
                for (uint i = 0; i < V2_RequestVariantCount2; i++)
                    V2_Variants2.Add(new MethodVariant(s, log, depth + 1, Version));
            }
            return this;
        }

        public bool ShouldSerializeV2_RequestVariantCount1()
        {
            return Version == 2;
        }

        public bool ShouldSerializeV2_RequestVariantCount2()
        {
            return Version == 2;
        }

        public bool ShouldSerializeV2_Variants1()
        {
            return Version == 2;
        }

        public bool ShouldSerializeV2_Variants2()
        {
            return Version == 2;
        }

        public bool ShouldSerializeNameSpace()
        {
            return Version == 1;
        }
    }
}
