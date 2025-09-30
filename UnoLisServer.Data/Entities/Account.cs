using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.Entities
{
    public class Account
    {
        [Key]
        [Column("idAccount")]
        public int AccountId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
    }
}
