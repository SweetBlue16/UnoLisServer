using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Contracts.DTOs
{
    public class ProfileData
    {
        public string Nickname { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int Level { get; set; }
        public int ExperiencePoints { get; set; }
        public int MatchesPlayed { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Streak { get; set; }
        public int MaxStreak { get; set; }
        public string CurrentAvatar { get; set; }
        public string FacebookUrl { get; set; }
        public string InstagramUrl { get; set; }
        public string TikTokUrl { get; set; }
        public string SelectedAvatarName { get; set; }
    }
}

