using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;

namespace UnoLisServer.Services.GameLogic.Models
{
    public class ItemUsageContext
    {
        public string LobbyCode { get; set; }
        public string ActorNickname { get; set; }
        public ItemType ItemType { get; set; }
        public string TargetNickname { get; set; }

        public ItemUsageContext(string lobbyCode, string actor, ItemType type, string target = null)
        {
            LobbyCode = lobbyCode;
            ActorNickname = actor;
            ItemType = type;
            TargetNickname = target;
        }
    }
}
