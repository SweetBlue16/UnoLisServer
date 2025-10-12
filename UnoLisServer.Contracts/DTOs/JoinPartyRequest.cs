using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Contracts.DTOs
{
    public class JoinPartyRequest
    {
        public string Nickname { get; set; }
        public string JoinCode { get; set; }
    }
}
