using System;
using System.Runtime.Serialization;

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
