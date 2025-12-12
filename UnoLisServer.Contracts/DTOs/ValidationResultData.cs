using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    //Object used to return the result of a friend request validation
    [DataContract]
    public class ValidationResultData
    {
        [DataMember]
        public FriendRequestResult Result { get; set; }

        [DataMember]
        public int RequesterId { get; set; }

        [DataMember]
        public int TargetId { get; set; }
    }
}