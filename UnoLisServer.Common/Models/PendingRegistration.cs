namespace UnoLisServer.Common.Models
{
    public class PendingRegistration
    {
        public string Nickname { get; set; }
        public string FullName { get; set; }
        public string HashedPassword { get; set; }
    }
}
