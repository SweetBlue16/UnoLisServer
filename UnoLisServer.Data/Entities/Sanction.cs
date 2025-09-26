using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.Entities
{
    public class Sanction
    {
        public int IdSanction { get; set; }
        public string SanctionType { get; set; }
        public string SanctionDescription { get; set; }
        public DateTime SanctionDate { get; set; }

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
    }
}
