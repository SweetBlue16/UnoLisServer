using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.Entities
{
    public class Achievement
    {
        public int IdAchievements { get; set; }
        public string AchievementTitle { get; set; }
        public string AchievementsDescription { get; set; }
        public string AchievementRarity { get; set; }
        public DateTime AchievementDate { get; set; }

        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
    }
}
