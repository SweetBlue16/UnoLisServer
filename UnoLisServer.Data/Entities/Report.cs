using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.Entities
{
    public class Report
    {
        public int IdReport { get; set; }
        public string ReportDescription { get; set; }
        public DateTime ReportDate { get; set; }

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
    }
}
