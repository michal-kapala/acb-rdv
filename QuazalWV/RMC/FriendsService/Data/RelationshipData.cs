using System.IO;

namespace QuazalWV
{
    public class RelationshipData : IData
    {
        public uint Pid { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Defines friend's current status.
        /// </summary>
        /// 0 - incoming invitation request
        /// 1 - outgoing invitation request
        /// 2 - friend (offline? blocked?)
        /// 3 - friend (online?)
        public byte ByRelationship { get; set; }
        public uint Details { get; set; }
        public byte Status { get; set; }

        public RelationshipData()
        {
            Details = 0;
            Status = 0;
        }

        public RelationshipData(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Pid = Helper.ReadU32(s);
            Name = Helper.ReadString(s);
            ByRelationship = Helper.ReadU8(s);
            Details = Helper.ReadU32(s);
            Status = Helper.ReadU8(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU32(s, Pid);
            Helper.WriteString(s, Name);
            Helper.WriteU8(s, ByRelationship);
            Helper.WriteU32(s, Details);
            Helper.WriteU8(s, Status);
        }
    }
}
