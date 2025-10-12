using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Contracts.DTOs
{
    public class PurchaseRequest
    {
        public string Nickname { get; set; }
        public int ItemId { get; set; }
    }
}
