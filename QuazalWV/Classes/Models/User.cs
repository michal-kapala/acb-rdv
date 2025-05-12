namespace QuazalWV
{
    public class User : DbModel
    {
        public uint UserDBPid { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string UserDBUbiId { get; set; }
        public string Email { get; set; }
        public string CountryCode { get; set; }
        public string PrefLang { get; set; }

        public override string ToString()
        {
            return $"UserDBPid {UserDBPid} Name {Name} UserDBUbiID {UserDBUbiId} ";
        }
    }
}
