using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.Entities
{
    public class AvatarsUnlocked
    {
        public int PlayerId { get; set; }
        public int AvatarId { get; set; }
        public DateTime UnlockedDate { get; set; }

        public virtual Player Player { get; set; }
        public virtual Avatar Avatar { get; set; }
    }
}
