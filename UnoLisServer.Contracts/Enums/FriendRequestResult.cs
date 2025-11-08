using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    [DataContract]
    public enum FriendRequestResult
    {
        [EnumMember] Success,
        [EnumMember] UserNotFound,
        [EnumMember] AlreadyFriends,
        [EnumMember] RequestAlreadySent,
        [EnumMember] Failed,
        [EnumMember] RequestAlreadyReceived,
        [EnumMember] CannotAddSelf
    }
}