using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.Entities
{
    public class SocialNetwork
    {
        public int IdSocialNetwork { get; set; }
        public string TypeSocialNetwork { get; set; }
        public string LinkSocialNetwork { get; set; }

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
    }
}
