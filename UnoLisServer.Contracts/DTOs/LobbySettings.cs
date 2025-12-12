using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Settings required previous to navigate to lobby
    /// </summary>
    [DataContract]
    public class LobbySettings
    {
        [DataMember]
        public string BackgroundVideoName { get; set; }

        [DataMember]
        public bool UseSpecialRules { get; set; }
    }
}