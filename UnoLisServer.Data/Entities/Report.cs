using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.Entities
{
    public class Report
    {
        [Key]
        [Column("idReport")]
        public int ReportId { get; set; }
        public string ReportDescription { get; set; }
        public DateTime ReportDate { get; set; }

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
    }
}
