using System;
using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Object for messages in chat
    /// </summary>
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
