using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Contracts.DTOs
{
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
