using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Represents a player avatar, including its identity, descriptive information, rarity, and selection status.
    /// </summary>
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
