namespace QuazalWV
{
    public class User : DbModel
    {
        public uint UserDBPid { get; set; }
        public string Name { get; set; }
        public byte[] Hash { get; set; }
        public byte[] Salt { get; set; }
        public string UbiId { get; set; }
        public string Email { get; set; }
        public string CountryCode { get; set; }
        public string PrefLang { get; set; }
    }
}
