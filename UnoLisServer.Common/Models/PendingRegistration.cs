namespace UnoLisServer.Common.Models
{
    /// <summary>
    /// Represents a user registration that is pending completion or approval.
    /// </summary>
    public class PendingRegistration
    {
        public string Nickname { get; set; }
        public string FullName { get; set; }
        public string HashedPassword { get; set; }
    }
}
