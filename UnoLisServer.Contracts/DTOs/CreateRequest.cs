using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Contracts.DTOs
{
    public class CreateRequestDto
    {
        public int RequesterId { get; set; }
        public int TargetId { get; set; }
        public string RequesterNickname { get; set; }
        public string TargetNickname { get; set; }
    }
}
