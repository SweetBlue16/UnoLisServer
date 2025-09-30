using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.Entities
{
    public class PlayerStatistics
    {
        [Key]
        [Column("idPlayerStatistics")]
        public int PlayerStatisticsId { get; set; }
        public int MatchesPlayed { get; set; }
        public int Wins { get; set; }
        public int Loses { get; set; }
        public int GlobalPoints { get; set; }
        public int Streak { get; set; }
        public int MaxStreak { get; set; }

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
    }
}
