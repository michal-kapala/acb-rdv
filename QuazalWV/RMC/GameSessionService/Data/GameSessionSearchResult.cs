using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuazalWV
{
	public class GameSessionSearchResult : IData
	{
		public GameSessionKey Key { get; set; }
		public uint HostPid { get; set; }
		public List<StationUrl> HostUrls { get; set; }
		public List<Property> Attributes { get; set; }

		public GameSessionSearchResult()
		{
            
        }

		public GameSessionSearchResult(Stream s)
		{
			HostUrls = new List<StationUrl>();
			Attributes = new List<Property>();
            FromStream(s);
		}

		public void FromStream(Stream s)
		{
			Key = new GameSessionKey(s);
			HostPid = Helper.ReadU32(s);
			uint count = Helper.ReadU32(s);
			for (uint i = 0; i < count; i++)
				HostUrls.Add(new StationUrl(Helper.ReadString(s)));
			
			count = Helper.ReadU32(s);
			for (uint i = 0; i < count; i++)
				Attributes.Add(new Property(s));
		}

		public void ToBuffer(Stream s)
		{
			Key.ToBuffer(s);
			Helper.WriteU32(s, HostPid);
			Helper.WriteU32(s, (uint)HostUrls.Count);
			foreach (StationUrl url in HostUrls)
				Helper.WriteString(s, url.ToString());
			Helper.WriteU32(s, (uint)Attributes.Count);
			foreach (Property prop in Attributes)
				prop.ToBuffer(s);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"[Session ID: {Key.SessionId}]");
			sb.AppendLine($"[Host: {HostPid}]");
			foreach (var prop in Attributes)
				sb.AppendLine($"\t{prop}");
			foreach (var url in HostUrls)
				sb.AppendLine($"[HostURL: {url}]");
			return sb.ToString();
		}
	}
}
