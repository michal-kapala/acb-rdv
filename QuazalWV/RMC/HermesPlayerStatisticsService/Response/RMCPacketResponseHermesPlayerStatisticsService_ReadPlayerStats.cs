using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
	public class RMCPacketResponseHermesPlayerStatisticsService_ReadPlayerStats : RMCPResponse
	{
		public List<PlayerStatContainer> Containers { get; set; }

		public RMCPacketResponseHermesPlayerStatisticsService_ReadPlayerStats(ClientInfo client, List<StatQuery> queries)
		{
			Containers = new List<PlayerStatContainer>
			{
				new PlayerStatContainer(client.User.Name, queries)
				{
					UnkUint = 0x2E5FF0
				}
			};
		}

		public override string ToString()
		{
			return "[ReadPlayerStats Response]";
		}

		public override string PayloadToString()
		{
			return $"\t[Containers: {Containers.Count}]";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Helper.WriteU32(m, (uint)Containers.Count);
			foreach (PlayerStatContainer container in Containers)
				container.ToBuffer(m);
			return m.ToArray();
		}
	}
}
