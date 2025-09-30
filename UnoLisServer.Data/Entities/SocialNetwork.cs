using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.Entities
{
    public class SocialNetwork
    {
        [Key]
        [Column("idSocialNetwork")]
        public int SocialNetworkId { get; set; }
        public string TypeSocialNetwork { get; set; }
        public string LinkSocialNetwork { get; set; }

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
    }
}
