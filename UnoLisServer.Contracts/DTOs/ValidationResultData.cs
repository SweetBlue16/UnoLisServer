using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
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