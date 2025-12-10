using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
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