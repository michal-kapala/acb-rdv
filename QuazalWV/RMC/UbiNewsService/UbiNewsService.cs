using System.Drawing;
using System.IO;

namespace QuazalWV
{
	public static class UbiNewsService
	{
		public static void ProcessRequest(Stream s, RMCP rmc)
		{
			switch (rmc.methodID)
			{
				case 1:
					// Empty GetNewsChannel request
					break;
				default:
					Log.WriteLine(1, $"[RMC UbiNews] Error: Unknown Method {rmc.methodID}", Color.Red);
					break;
			}
		}

		public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
		{
			RMCPResponse reply;
			switch (rmc.methodID)
			{
				case 1:
					reply = new RMCPacketResponseUbiNewsService_GetNewsChannel(client.User.UserDBPid);
					RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
					break;
				default:
					Log.WriteLine(1, $"[RMC UbiNews] Error: Unknown Method {rmc.methodID}", Color.Red, client);
					break;
			}
		}
	}
}
