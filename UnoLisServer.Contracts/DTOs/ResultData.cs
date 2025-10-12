using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Contracts.DTOs
{
    public class ResultData
    {
        public string Nickname { get; set; }
        public int Score { get; set; }
        public int Position { get; set; }
        public bool IsWinner { get; set; }
    }
}
