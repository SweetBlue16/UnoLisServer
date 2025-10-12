using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Contracts.DTOs
{
    public class PartyActionData
    {
        public int PartyId { get; set; }
        public string Nickname { get; set; }
        public bool? IsReady { get; set; }
    }
}

