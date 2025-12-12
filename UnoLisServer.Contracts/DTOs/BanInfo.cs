using System;
using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Information needed for a ban
    /// </summary>
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
