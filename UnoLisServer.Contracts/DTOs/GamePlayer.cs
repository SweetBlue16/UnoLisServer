using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    [DataContract]
    public class GamePlayer
    {
        [DataMember]
        public string Nickname { get; set; }

        [DataMember]
        public string AvatarName { get; set; }

        [DataMember]
        public int CardCount { get; set; }

        [DataMember]
        public bool IsConnected { get; set; } = true;
    }
}
