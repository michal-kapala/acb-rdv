using System.Drawing;
using System.IO;

namespace QuazalWV
{
	public static class RichPresenceService
	{
		public static void ProcessRequest(Stream s, RMCP rmc)
		{
			switch (rmc.methodID)
			{
				case 1:
					rmc.request = new RMCPacketRequestRichPresenceService_SetPresence(s);
					break;
				case 2:
					rmc.request = new RMCPacketRequestRichPresenceService_GetPresence(s);
					break;
				default:
					Log.WriteLine(1, $"[RMC RichPresence] Error: Unknown Method {rmc.methodID}", Color.Red);
					break;
			}
		}

		public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
		{
			RMCPResponse reply;
			switch (rmc.methodID)
			{
				case 1:
                    Log.WriteLine(1, $"[RMC RichPresence] richpresence {rmc.methodID}", Color.Red, client);
                    reply = new RMCPResponseEmpty();
					RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
					break;
				case 2:
                    Log.WriteLine(1, $"[RMC RichPresence] richpresence {rmc.methodID}", Color.Red, client);
                    reply = new RMCPacketResponseRichPresenceService_GetPresence();
					RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
					break;
				default:
					Log.WriteLine(1, $"[RMC RichPresence] Error: Unknown Method {rmc.methodID}", Color.Red, client);
					break;
			}
		}
	}
}
