using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.Entities
{
    public class Avatar
    {
        public int IdAvatar { get; set; }
        public string AvatarName { get; set; }
        public string AvatarDescription { get; set; }
        public string AvatarRarity { get; set; }

        public virtual ICollection<AvatarsUnlocked> PlayersUnlocked { get; set; }
    }
}
