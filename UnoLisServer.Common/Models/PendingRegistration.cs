using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Common.Models
{
    public class PendingRegistration
    {
        public string Nickname { get; set; }
        public string FullName { get; set; }
        public string HashedPassword { get; set; }
    }
}
