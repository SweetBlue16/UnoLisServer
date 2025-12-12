using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Information to create a frienshipRequest
    /// </summary>
    [DataContract]
    public class FriendRequestData
    {
        [DataMember]
        public string RequesterNickname { get; set; }

        [DataMember]
        public string TargetNickname { get; set; }

        [DataMember]
        public int FriendListId { get; set; }
    }
}
