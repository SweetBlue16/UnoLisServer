using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace UnoLisServer.Data.Entities
{
    public class Player
    {
        public int IdPlayer { get; set; }
        public string Nickname { get; set; }
        public string FullName { get; set; }

        public virtual Account Account { get; set; }
        public virtual PlayerStatistics Statistics { get; set; }

        public virtual ICollection<Sanction> Sanctions { get; set; }
        public virtual ICollection<SocialNetwork> SocialLinks { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<Achievement> Achievements { get; set; }

        public virtual ICollection<AvatarsUnlocked> AvatarsUnlocked { get; set; }
        public virtual ICollection<FriendList> Friends { get; set; }
    }
}
