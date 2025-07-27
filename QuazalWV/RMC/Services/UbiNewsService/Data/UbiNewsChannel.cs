using System.IO;

namespace QuazalWV
{
	public class UbiNewsChannel : IData
	{
		public Gathering Gathering { get; set; }
		public string Name { get; set; }
		public string ClassName { get; set; }
		public string Locale { get; set; }
		public bool Subscribable { get; set; }

		public UbiNewsChannel(uint pid)
		{
			Gathering = new Gathering()
			{
				m_idMyself = 2,
				m_pidOwner = pid,
				m_pidHost = 0,
				m_uiMinParticipants = 0,
				m_uiMaxParticipants = 64,
				m_uiParticipationPolicy = (uint)PARTICIPATION_POLICY.NewsChannelDefault,
				m_uiPolicyArgument = 0,
				m_uiFlags = 1,
				m_uiState = 0,
				m_strDescription = "Description"
			};
			Name = "Name";
			ClassName = "UbiNews";
			Locale = "en-US";
			Subscribable = false;
		}

		public UbiNewsChannel(Stream s)
		{
			Gathering = new Gathering();
			FromStream(s);
		}

		public void FromStream(Stream s)
		{
			Gathering.FromStream(s);
			Name = Helper.ReadString(s);
			ClassName = Helper.ReadString(s);
			Locale = Helper.ReadString(s);
			Subscribable = Helper.ReadBool(s);
		}

		public void ToBuffer(Stream s)
		{
			Gathering.ToBuffer(s);
			Helper.WriteString(s, Name);
			Helper.WriteString(s, ClassName);
			Helper.WriteString(s, Locale);
			Helper.WriteBool(s, Subscribable);
		}
	}
}
