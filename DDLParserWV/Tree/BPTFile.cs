using System.Collections.Generic;
using Newtonsoft.Json;

namespace DDLParserWV
{
    /// <summary>
    /// The root object of output JSON.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class BPTFile
    {
        [JsonProperty("file")]
        public string Name { get; set; }
        [JsonProperty("trees")]
        public List<ParseTree> ParseTrees { get; set; } = new List<ParseTree>();

        public BPTFile(string name)
        {
            Name = name;
        }
    }
}
