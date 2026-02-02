using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DDLParserWV
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TemplateDeclarationUse
    {
        [JsonProperty("type")]
        public string TypeName {  get; set; }
        [JsonProperty("nsItem")]
        public NameSpaceItem NsItem {  get; set; } = new NameSpaceItem();
        [JsonProperty("declUses")]
        public List<DeclarationUse> DeclarationUses { get; set; } = new List<DeclarationUse>();

        public TemplateDeclarationUse Parse(Stream s, StringBuilder log, uint depth, uint majorVersion)
        {
            // NameSpaceItem data inconsistent in v2 (no added uint fields, same as v1)
            //NsItem.Parse(s, log, depth + 1, majorVersion);
            NsItem.TreeItemName = Utils.ReadString(s);
            NsItem.NsItemName = Utils.ReadString(s);
            TypeName = NsItem.TreeItemName;
            byte count = (byte)s.ReadByte();
            byte type;
            for (byte i = 0; i < count; i++)
            {
                type = (byte)s.ReadByte();
                DeclarationUses.Add(new DeclarationUse(s, (EParseTreeElement)type, log, depth + 1, majorVersion));
            }
            return this;
        }
    }
}
