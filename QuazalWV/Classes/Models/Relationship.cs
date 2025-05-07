namespace QuazalWV
{
    public class Relationship
    {
        public uint RequesterPid { get; set; }
        public uint RequesteePid { get; set; }
        public DbPlayerRelationshipType Type { get; set; }
        public uint Details { get; set; }

        public Relationship()
        {

        }

        public Relationship(RelationshipData data, uint ownPid)
        {
            RequesterPid = ownPid;
            RequesteePid = data.Pid;
            Type = (DbPlayerRelationshipType)data.ByRelationship;
            Details = data.Details;
        }
    }
}
