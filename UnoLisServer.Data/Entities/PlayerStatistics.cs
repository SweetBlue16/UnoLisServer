using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.Entities
{
    public class PlayerStatistics
    {
        public int IdPlayerStatistics { get; set; }
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
