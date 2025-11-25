using System;
using System.Collections.Generic;
namespace UnoLisServer.Contracts.DTOs
{
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string Nickname { get; set; }
        public int Wins { get; set; }
        public int MatchesPlayed { get; set; }
        public int GlobalPoints { get; set; }
        public string WinRate { get; set; }
    }
}
