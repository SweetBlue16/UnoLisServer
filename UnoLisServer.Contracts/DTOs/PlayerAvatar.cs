using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    [DataContract]
    public class PlayerAvatar
    {
        [DataMember]
        public int AvatarId { get; set; }
        [DataMember]
        public string AvatarName { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Rarity { get; set; }
        [DataMember]
        public bool IsUnlocked { get; set; }
        [DataMember]
        public bool IsSelected { get; set; }
    }
}
