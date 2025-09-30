using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.Entities
{
    public class Avatar
    {
        [Key]
        [Column("idAvatar")]
        public int AvatarId { get; set; }
        public string AvatarName { get; set; }
        public string AvatarDescription { get; set; }
        public string AvatarRarity { get; set; }

        public virtual ICollection<AvatarsUnlocked> PlayersUnlocked { get; set; }
    }
}
