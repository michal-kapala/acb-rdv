using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuazalWV
{
	public class RMCPacketRequestNATTraversalService_RequestProbeInitiation : RMCPRequest
	{
		public List<StationUrl> TargetUrls { get; set; }

		public RMCPacketRequestNATTraversalService_RequestProbeInitiation(Stream s)
		{
			TargetUrls = new List<StationUrl>();
			var urls = Helper.ReadStringList(s);
			foreach (var url in urls)
				TargetUrls.Add(new StationUrl(url));
		}

		public override string PayloadToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var url in TargetUrls)
				sb.AppendLine($"[URL: {url.ToString()}]");

			return sb.ToString();
		}

		public override byte[] ToBuffer()
		{
			var urls = new List<string>();
			foreach (var url in TargetUrls)
				urls.Add(url.ToString());

			MemoryStream m = new MemoryStream();
			Helper.WriteStringList(m, urls);
			return m.ToArray();
		}

		public override string ToString()
		{
			return "[RequestProbeInitiation Request]";
		}
	}
}
