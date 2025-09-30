using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.Entities
{
    public class Sanction
    {
        [Key]
        [Column("idSanction")]
        public int SanctionId { get; set; }
        public string SanctionType { get; set; }
        public string SanctionDescription { get; set; }
        public DateTime SanctionDate { get; set; }

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
    }
}
