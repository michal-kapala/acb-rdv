namespace QuazalWV
{
    public class GameInvite : DbModel
    {
        public uint Inviter { get; set; }
        public GameSessionInvitation Invitation { get; set; }
    }
}
