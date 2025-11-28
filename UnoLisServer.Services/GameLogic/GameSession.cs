using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers; 
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Enums; 
using UnoLisServer.Services.GameLogic.Models;
using UnoLisServer.Common.Helpers;

namespace UnoLisServer.Services.GameLogic
{
    public class GameSession : IDisposable
    {
        public string LobbyCode { get; }

        public DeckManager Deck { get; }
        public List<GamePlayerData> Players { get; }

        public int CurrentTurnIndex { get; private set; }
        public bool IsClockwise { get; private set; } = true;
        public CardColor CurrentActiveColor { get; set; } 

        public DateTime? PenultimateCardPlayedTime { get; set; }

        public readonly object GameLock = new object();

        private readonly Timer _turnTimer;
        private const int TurnDurationSeconds = 30;

        public event Action<string> OnTurnTimeExpired;

        public GameSession(string lobbyCode, List<string> playerNicknames)
        {
            LobbyCode = lobbyCode;
            Deck = new DeckManager();
            Players = new List<GamePlayerData>();

            foreach (var nick in playerNicknames)
            {
                Players.Add(new GamePlayerData(nick));
            }

            _turnTimer = new Timer(TurnDurationSeconds * 1000);
            _turnTimer.AutoReset = false; 
            _turnTimer.Elapsed += TurnTimer_Elapsed;
        }

        public void StartGame()
        {
            lock (GameLock)
            {
                foreach (var player in Players)
                {
                    player.Hand.AddRange(Deck.DrawCards(7));
                }

                var initialCard = Deck.StartDiscardPile();
                if (initialCard.Color == CardColor.Wild)
                {
                    CurrentActiveColor = CardColor.Red;
                }
                else
                {
                    CurrentActiveColor = initialCard.Color;
                }

                CurrentTurnIndex = 0;
                StartTurnTimer();
            }
        }
        public void NextTurn()
        {
            lock (GameLock)
            {
                var currentPlayer = GetCurrentPlayer();
                currentPlayer.HasDrawnThisTurn = false;

                if (IsClockwise)
                {
                    CurrentTurnIndex = (CurrentTurnIndex + 1) % Players.Count;
                }
                else
                {
                    CurrentTurnIndex = (CurrentTurnIndex - 1 + Players.Count) % Players.Count;
                }

                StartTurnTimer();
            }
        }

        public void ReverseDirection()
        {
            IsClockwise = !IsClockwise;
        }

        public void SkipTurn()
        {
            NextTurn();
        }

        public GamePlayerData GetCurrentPlayer()
        {
            return Players[CurrentTurnIndex];
        }

        public GamePlayerData GetPlayer(string nickname)
        {
            return Players.FirstOrDefault(p => p.Nickname == nickname);
        }

        public void StartTurnTimer()
        {
            _turnTimer.Stop();
            _turnTimer.Start();
        }

        public void StopTurnTimer()
        {
            _turnTimer.Stop();
        }

        private void TurnTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var currentPlayerNick = GetCurrentPlayer().Nickname;
            OnTurnTimeExpired?.Invoke(currentPlayerNick);
        }

        public void Dispose()
        {
            _turnTimer.Stop();
            _turnTimer.Dispose();
        }
    }
}