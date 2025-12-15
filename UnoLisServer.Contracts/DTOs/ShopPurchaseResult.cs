using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Data transfer object to manage an avatar purchase in the shop
    /// </summary>
    [DataContract]
    public class ShopPurchaseResult
    {
        [DataMember]
        public bool IsSuccess { get; set; }

        [DataMember]
        public string MessageCode { get; set; } 

        [DataMember]
        public PlayerAvatar WonAvatar { get; set; } 

        [DataMember]
        public int RemainingCoins { get; set; }
    }
}
