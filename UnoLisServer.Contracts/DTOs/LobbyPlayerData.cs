using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Information to show players in lobby 
    /// </summary>
    [DataContract]
    public class LobbyPlayerData
    {
        [DataMember]
        public string Nickname { get; set; }

        [DataMember]
        public string AvatarName { get; set; }

        [DataMember]
        public bool IsReady { get; set; }
    }
}