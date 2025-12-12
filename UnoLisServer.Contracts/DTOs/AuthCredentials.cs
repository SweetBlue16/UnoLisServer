namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Credentials for authentication for session
    /// </summary>
    public class AuthCredentials
    {
        public string Nickname { get; set; }
        public string Password { get; set; }
    }
}
