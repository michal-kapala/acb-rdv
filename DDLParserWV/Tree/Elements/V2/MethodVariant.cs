using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace DDLParserWV
{
    /// <summary>
    /// Only present in v2.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class MethodVariant
    {
        [JsonProperty("id")]
        public uint Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("namespace")]
        public NameSpace NameSpace { get; set; }
        private uint Version { get; set; }

        public MethodVariant(Stream s, StringBuilder log, uint depth, uint majorVersion)
        {
            Version = majorVersion;
            if (Version == 1)
                throw new NotImplementedException("MethodVariant is only used in v2");
            string tabs = Utils.MakeTabs(depth);
            log.AppendLine($"{tabs}[MethodVariant]");
            Id = Utils.ReadU32(s);
            log.AppendLine($"{tabs}\t[Id: {Id}]");
            Name = Utils.ReadString(s);
            log.AppendLine($"{tabs}\t[Name: {Name}]");
            NameSpace = new NameSpace(s, log, depth + 1, Version);
        }
    }
}
