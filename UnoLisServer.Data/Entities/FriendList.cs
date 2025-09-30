using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.Entities
{
    public class FriendList
    {
        [Key]
        [Column("idFriendList")]
        public int FriendListId { get; set; }

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }

        public int FriendId { get; set; }
        public virtual Player Friend { get; set; }

        public bool FriendRequest { get; set; } 
    }
}
