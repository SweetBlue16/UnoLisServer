using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.Entities
{
    public class Account
    {
        public int IdAccount { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
    }
}
