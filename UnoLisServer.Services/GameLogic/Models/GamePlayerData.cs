using System.Collections.Generic;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Services.GameLogic.Models
{
    public class GamePlayerData
    {
        public string Nickname { get; set; }
        public string AvatarName { get; set; }
        public List<Card> Hand { get; set; }
        public bool HasSaidUno { get; set; }
        public bool HasDrawnThisTurn { get; set; }

        public GamePlayerData(string nickname, string avatarName)
        {
            Nickname = nickname;
            AvatarName = avatarName;
            Hand = new List<Card>();
            HasSaidUno = false;
            
        }
    }
}