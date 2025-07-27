using System.IO;

namespace QuazalWV
{
    public enum PARTICIPATION_POLICY
    {
        Open = 1,
        Group,
        OnInvitation,
        PasswordProtected,
        CapabilityBased,
        Unknown1,
        Unknown2,
        Unknown3,
        NewsChannelDefault,
    }

    public class Gathering : IData
    {
        public uint m_idMyself;
        public uint m_pidOwner;
        public uint m_pidHost;
        public ushort m_uiMinParticipants;
        public ushort m_uiMaxParticipants;
        public uint m_uiParticipationPolicy;
        public uint m_uiPolicyArgument;
        public uint m_uiFlags;
        public uint m_uiState;
        public string m_strDescription;

        public Gathering()
        {
            m_idMyself = Global.GathIdCounter++;
            m_pidHost = 0;
            m_pidOwner = 0;
            m_uiMinParticipants = 0;
            m_uiMaxParticipants = 8;
            m_uiParticipationPolicy = (uint)PARTICIPATION_POLICY.Open;
            m_uiPolicyArgument = 0;
            m_uiFlags = 1;
            m_uiState = 0;
            m_strDescription = "";
        }

        public Gathering(Stream s)
        {
            FromStream(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU32(s, m_idMyself);
            Helper.WriteU32(s, m_pidOwner);
            Helper.WriteU32(s, m_pidHost);
            Helper.WriteU16(s, m_uiMinParticipants);
            Helper.WriteU16(s, m_uiMaxParticipants);
            Helper.WriteU32(s, m_uiParticipationPolicy);
            Helper.WriteU32(s, m_uiPolicyArgument);
            Helper.WriteU32(s, m_uiFlags);
            Helper.WriteU32(s, m_uiState);
            Helper.WriteString(s, m_strDescription);
        }

        public void FromStream(Stream s)
        {
            m_idMyself = Helper.ReadU32(s);
            m_pidOwner = Helper.ReadU32(s);
            m_pidHost = Helper.ReadU32(s);
            m_uiMinParticipants = Helper.ReadU16(s);
            m_uiMaxParticipants = Helper.ReadU16(s);
            m_uiParticipationPolicy = Helper.ReadU32(s);
            m_uiPolicyArgument = Helper.ReadU32(s);
            m_uiFlags = Helper.ReadU32(s);
            m_uiState = Helper.ReadU32(s);
            m_strDescription = Helper.ReadString(s);
        }
    }
}
