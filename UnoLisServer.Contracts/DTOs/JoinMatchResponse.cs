using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Data Transfer Object sent from the server after a join match attempt.
    /// </summary>
    [DataContract]
    public class JoinMatchResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string LobbyCode { get; set; }
    }
}
