using System;
using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    [DataContract]
    public class BanInfo
    {
        [DataMember]
        public string Reason { get; set; }

        [DataMember]
        public DateTime EndDate { get; set; }

        [DataMember]
        public double RemainingHours { get; set; }

        [DataMember]
        public string FormattedTimeRemaining { get; set; }
    }
}
