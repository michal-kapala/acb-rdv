using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
    public class UbiAccount : IData
    {
        public string UbiAccountId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public UbiAccountStatus Status { get; set; }
        public string Email { get; set; }
        public ulong DateOfBirth { get; set; }
        public uint Gender { get; set; }
        public string CountryCode { get; set; }
        public bool OptIn { get; set; }
        public bool ThirdPartyOptIn { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PreferredLanguage { get; set; }
        public List<ExternalAccount> ExternalAccounts { get; set; }

        public UbiAccount()
        {
            // password can be blank
            Password = "";
            // default account status
            Status = new UbiAccountStatus();
            // September 27th, 2000
            DateOfBirth = 0x1F42760000;
            // unspecified
            Gender = 0;
            OptIn = false;
            ThirdPartyOptIn = false;
            // names can be blank
            FirstName = "";
            LastName = "";
            PreferredLanguage = "en";
            ExternalAccounts = new List<ExternalAccount>();
        }

        public UbiAccount(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            UbiAccountId = Helper.ReadString(s);
            Username = Helper.ReadString(s);
            Password = Helper.ReadString(s);
            Status = new UbiAccountStatus(s);
            Email = Helper.ReadString(s);
            DateOfBirth = Helper.ReadU64(s);
            Gender = Helper.ReadU32(s);
            CountryCode = Helper.ReadString(s);
            OptIn = Helper.ReadBool(s);
            ThirdPartyOptIn = Helper.ReadBool(s);
            FirstName = Helper.ReadString(s);
            LastName = Helper.ReadString(s);
            PreferredLanguage = Helper.ReadString(s);
            ExternalAccounts = new List<ExternalAccount>();
            uint accCount = Helper.ReadU32(s);
            for (int i = 0; i < accCount; i++)
                ExternalAccounts.Add(new ExternalAccount(s));
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteString(s, UbiAccountId);
            Helper.WriteString(s, Username);
            Helper.WriteString(s, Password);
            Status.ToBuffer(s);
            Helper.WriteString(s, Email);
            Helper.WriteU64(s, DateOfBirth);
            Helper.WriteU32(s, Gender);
            Helper.WriteString(s, CountryCode);
            Helper.WriteBool(s, OptIn);
            Helper.WriteBool(s, ThirdPartyOptIn);
            Helper.WriteString(s, FirstName);
            Helper.WriteString(s, LastName);
            Helper.WriteString(s, PreferredLanguage);
            Helper.WriteU32(s, (uint)ExternalAccounts.Count);
            foreach (ExternalAccount ea in ExternalAccounts)
                ea.ToBuffer(s);
        }
    }
}
