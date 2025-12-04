using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    [DataContract]
    public class ResultData
    {
        [DataMember]
        public string Nickname { get; set; }

        [DataMember]
        public int Score { get; set; }

        [DataMember]
        public int Rank { get; set; }

        [DataMember]
        public bool AvatarName { get; set; }
    }
}