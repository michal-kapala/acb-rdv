namespace QuazalWV
{
    public class User : DbModel
    {
        public uint Pid { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string UbiId { get; set; }
        public string Email { get; set; }
        public string CountryCode { get; set; }
        public string PrefLang { get; set; }
    }
}
