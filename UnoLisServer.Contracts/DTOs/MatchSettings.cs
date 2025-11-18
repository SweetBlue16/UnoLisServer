using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Data Transfer Object sent by the client when creating a match.
    /// It contains the game rules and the host's identity.
    /// </summary>
    [DataContract]
    public class MatchSettings
    {
        [DataMember]
        public string HostNickname { get; set; }

        [DataMember]
        public int MaxPlayers { get; set; }

        [DataMember]
        public bool UseSpecialRules { get; set; }
    }
}