using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    [DataContract]
    public class FriendData
    {
        [DataMember]
        public string FriendNickname { get; set; }

        [DataMember]
        public bool IsOnline { get; set; }

        [DataMember]
        public string StatusMessage { get; set; }
    }
}
