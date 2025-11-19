using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    [DataContract]
    public class LobbyPlayerData
    {
        [DataMember]
        public string Nickname { get; set; }

        [DataMember]
        public string AvatarName { get; set; }
    }
}