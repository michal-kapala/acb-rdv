namespace QuazalWV
{
    public class RelationshipDBData : DbModel
	{
        public uint Pidrequestor { get; set; }
        public uint Pidrequestee { get; set; }
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

        public RelationshipDBData()
        {
            
        }

    }
}
