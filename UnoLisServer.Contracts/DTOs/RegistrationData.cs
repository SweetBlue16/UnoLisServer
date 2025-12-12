namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Represents the information to register a new player
    /// </summary>
    public class RegistrationData
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Nickname { get; set; }
        public string FullName { get; set; }
    }
}

