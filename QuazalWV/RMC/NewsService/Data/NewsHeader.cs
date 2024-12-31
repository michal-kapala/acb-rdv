using System.IO;

namespace QuazalWV
{
	public class NewsHeader : IData
	{
		public uint Id {  get; set; }
		public uint RecipientId { get; set; }
		public uint RecipientType { get; set; }
		public uint PublisherPid { get; set; }
		public string PublisherName { get; set; }
		public QDateTime PublicationTime { get; set; }
		public QDateTime DisplayTime { get; set; }
		public QDateTime ExpirationTime { get; set; }
		public string Title { get; set; }
		public string Link { get; set; }

		public NewsHeader()
		{

		}

		public void FromStream(Stream s)
		{
			Id = Helper.ReadU32(s);
			RecipientId = Helper.ReadU32(s);
			RecipientType = Helper.ReadU32(s);
			PublisherPid = Helper.ReadU32(s);
			PublisherName = Helper.ReadString(s);
			PublicationTime = new QDateTime(s);
			DisplayTime = new QDateTime(s);
			ExpirationTime = new QDateTime(s);
			Title = Helper.ReadString(s);
			Link = Helper.ReadString(s);
		}

		public void ToBuffer(Stream s)
		{
			Helper.WriteU32(s, Id);
			Helper.WriteU32(s, RecipientId);
			Helper.WriteU32(s, RecipientType);
			Helper.WriteU32(s, PublisherPid);
			Helper.WriteString(s, PublisherName);
			PublicationTime.ToBuffer(s);
			DisplayTime.ToBuffer(s);
			ExpirationTime.ToBuffer(s);
			Helper.WriteString(s, Title);
			Helper.WriteString(s, Link);
		}
	}
}
