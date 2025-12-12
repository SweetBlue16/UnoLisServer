using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Data Transfer Object sent by the client when creating a match.
    /// It contains the capacity and the host's identity.
    /// </summary>
    [DataContract]
    public class MatchSettings
    {
        [DataMember]
        public string HostNickname { get; set; }

        [DataMember]
        public int MaxPlayers { get; set; }

        [DataMember]
        public bool UseSpecialRules { get; set; }
    }
}