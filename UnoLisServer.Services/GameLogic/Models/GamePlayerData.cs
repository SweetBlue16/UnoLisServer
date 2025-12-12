using System.Collections.Generic;
using UnoLisServer.Common.Enums;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Services.GameLogic.Models
{
    /// <summary>
    /// Represents the state and attributes of a player in a game session, including identity, hand, status flags, and
    /// inventory.
    /// </summary>
    public class GamePlayerData
    {
        public string Nickname { get; set; }
        public string AvatarName { get; set; }
        public List<Card> Hand { get; set; }
        public bool HasSaidUno { get; set; }
        public bool HasDrawnThisTurn { get; set; }

        public bool IsConnected { get; set; } = true;

        public GamePlayerData()
        {
        }

        public GamePlayerData(string nickname, string avatarName)
        {
            Nickname = nickname;
            AvatarName = avatarName;
            Hand = new List<Card>();
            HasSaidUno = false;
        }

        public Dictionary<ItemType, int> Items { get; set; } = new Dictionary<ItemType, int>();
    }
}