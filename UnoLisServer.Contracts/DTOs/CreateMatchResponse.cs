using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Data Transfer Object sent from the server after a match creation attempt.
    /// Contains the result and the new lobby code.
    /// </summary>
    [DataContract]
    public class CreateMatchResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string LobbyCode { get; set; }
    }
}