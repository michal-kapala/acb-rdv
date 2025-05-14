namespace QuazalWV
{
    public class Relationship
    {
        public uint RequesterPid { get; set; }
        public uint RequesteePid { get; set; }
        public PlayerRelationship Type { get; set; }
        public uint Details { get; set; }

        public Relationship()
        {

        }

        public Relationship(RelationshipData data, uint ownPid)
        {
            RequesterPid = ownPid;
            RequesteePid = data.Pid;
            Type = (PlayerRelationship)data.ByRelationship;
            Details = data.Details;
        }
    }
}
