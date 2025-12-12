using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Object to handle player in match
    /// </summary>
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
