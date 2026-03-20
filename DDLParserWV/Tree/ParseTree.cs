using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace DDLParserWV
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ParseTree
    {
        /// <summary>
        /// Unknown byte, usually 0.
        /// </summary>
        [JsonProperty("unusedByte")]
        public byte UnusedByte { get; set; }
        /// <summary>
        /// Semver with build id added at the end.
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("namespace")]
        public NameSpace GlobalNamespace {  get; set; }
        [JsonProperty("unknown_v2")]
        public uint V2_Unknown { get; set; }
        public uint MajorVersion { get; set; }
        public uint MinorVersion { get; set; }
        public uint PatchVersion { get; set; }

        public ParseTree(Stream s, StringBuilder log)
        {
            log.AppendLine("[ParseTree]");
            UnusedByte = (byte)s.ReadByte();
            log.AppendLine($"\t[unusedByte: {UnusedByte}]");
            MajorVersion = Utils.ReadU32(s);
            MinorVersion = Utils.ReadU32(s);
            PatchVersion = Utils.ReadU32(s);
            uint build = Utils.ReadU32(s);
            Version = $"{MajorVersion}.{MinorVersion}.{PatchVersion}.{build}";
            log.AppendLine($"\t[version: {Version}]");
            // only present in v2 < 2.5.7
            if (MajorVersion == 2 && (MinorVersion < 5 || (MinorVersion == 5 && PatchVersion < 7)))
                V2_Unknown = Utils.ReadU32(s);
            GlobalNamespace = new NameSpace(s, log, 1, MajorVersion);
            while ((s.Position % 4) != 0)
                s.ReadByte();
        }

        public bool ShouldSerializeV2_Unknown()
        {
            return MajorVersion == 2 && (MinorVersion < 5 || (MinorVersion == 5 && PatchVersion < 7));
        }
    }
}
