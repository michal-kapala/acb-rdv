using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketResponseFriendsService_GetRelationships : RMCPResponse
    {
        public List<RelationshipData> Relationships { get; set; }

        public RMCPacketResponseFriendsService_GetRelationships(List<RelationshipData> rdata)
        {
            Relationships = rdata;
        }

        public override string ToString()
        {
            return "[GetRelationships Response]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Relationships: {Relationships.Count}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)Relationships.Count);
            foreach(RelationshipData r in Relationships)
                r.ToBuffer(m);
            return m.ToArray();
        }
    }
}
