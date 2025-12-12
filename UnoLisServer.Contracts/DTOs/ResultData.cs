using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Match result data 
    /// </summary>
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
        public string AvatarName { get; set; }
    }
}