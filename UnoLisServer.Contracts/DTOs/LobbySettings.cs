using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    [DataContract]
    public class LobbySettings
    {
        [DataMember]
        public string BackgroundVideoName { get; set; }

        [DataMember]
        public bool UseSpecialRules { get; set; }
    }
}