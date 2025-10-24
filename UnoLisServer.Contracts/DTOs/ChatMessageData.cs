using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Contracts.DTOs
{
    public class ChatMessageData
    {
        [DataMember]
        public string Nickname { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [DataMember] 
        public string ChannelId { get; set; }
    }
}
