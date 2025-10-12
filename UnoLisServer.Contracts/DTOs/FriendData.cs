using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Contracts.DTOs
{
    public class FriendData
    {
        public string FriendNickname { get; set; }
        public bool IsOnline { get; set; }
        public string StatusMessage { get; set; }
    }
}
