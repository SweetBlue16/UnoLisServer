using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Contracts.DTOs
{
    public class FriendRequestData
    {
        public string RequesterNickname { get; set; }
        public string TargetNickname { get; set; }
        public int FriendListId { get; set; }
    }
}
