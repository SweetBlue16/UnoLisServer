using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Enums;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Services.GameLogic;
using UnoLisServer.Services.GameLogic.Models;
using UnoLisServer.Services.Helpers;
using UnoLisServer.Services.ManagerInterfaces;

namespace UnoLisServer.Services
{
    public class GameManager : IGameManager
    {
        private readonly GameSessionHelper _sessionHelper;

        public GameManager() : this(GameSessionHelper.Instance) 
        {
        }

        public GameManager(GameSessionHelper sessionHelper)
        {
            _sessionHelper = sessionHelper;
        }

        public bool InitializeGame(string lobbyCode, List<string> playerNicknames)
        {
            try
            {
                var session = new GameSession(lobbyCode, playerNicknames);
                session.OnTurnTimeExpired += async (nick) => await HandleTurnExpired(lobbyCode, nick);
                session.StartGame();
                _sessionHelper.CreateGame(lobbyCode, session);

                return true;
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed connecting player (Communication): {commEx.Message}");
                return false;
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout connecting the player: {timeEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error initializing game {lobbyCode}", ex);
                return false;
            }
        }

        public void ConnectPlayer(string lobbyCode, string nickname)
        {
            if (OperationContext.Current == null)
            {
                Logger.Warn($"[GAME] ConnectPlayer called without OperationContext for {nickname}");
                return;
            }

            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<IGameplayCallback>();
                _sessionHelper.RegisterCallback(lobbyCode, nickname, callback);

                var session = _sessionHelper.GetGame(lobbyCode);
                if (session != null)
                {
                    var player = session.GetPlayer(nickname);

                    if (player == null)
                    {
                        Logger.Warn($"[GAME] Player {nickname} not found in session {lobbyCode}");
                        return;
                    }

                    System.Threading.Thread.Sleep(500);
                    SendInitialStateToPlayer(callback, session, player);
                }
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed connecting player (Communication): {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout connecting the player: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Critical error connecting player {nickname}", ex);
            }
        }



        public void DisconnectPlayer(string lobbyCode, string nickname)
        {
            _sessionHelper.UnregisterCallback(lobbyCode, nickname);
        }

        public Task PlayCardAsync(PlayCardContext context)
        {
            try
            {
                var session = _sessionHelper.GetGame(context.LobbyCode);
                if (session == null) return Task.CompletedTask;

                lock (session.GameLock)
                {
                    var player = session.GetPlayer(context.Nickname);
                    var cardToPlay = player?.Hand.FirstOrDefault(c => c.Id == context.CardId);

                    if (!ValidatePlay(session, player, cardToPlay))
                    {
                        Logger.Warn($"[GAME] Invalid play attempt by {context.Nickname} in {context.LobbyCode}");
                        return Task.CompletedTask;
                    }

                    ProcessCardMove(session, cardToPlay, context);
                    NotifyPlay(context.LobbyCode, context.Nickname, cardToPlay);

                    if (player.Hand.Count == 0)
                    {
                        HandleWinCondition(session, player);
                        return Task.CompletedTask;
                    }

                    AdvanceTurnLogic(session, cardToPlay, context.LobbyCode);
                }
            }
            catch (ArgumentOutOfRangeException argEx)
            {
                Logger.Warn($"[GAME] Invalid color selection for {context.Nickname}: {argEx.Message}");
            }
            catch (NullReferenceException nullEx)
            {
                Logger.Error($"[GAME] State corruption in PlayCard for {context.Nickname}", nullEx);
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Critical error in PlayCard for {context.Nickname}", ex);
            }

            return Task.CompletedTask;
        }

        private bool IsValidMove(Card card, GameSession session)
        {
            var topCard = session.Deck.PeekTopCard();
            if (IsWildCard(card.Value))
            {
                return true;
            }

            if (card.Color == session.CurrentActiveColor)
            {
                return true;
            }

            if (topCard != null && card.Value == topCard.Value)
            {
                return true;
            }

            return false;
        }

        private bool ApplyCardEffects(GameSession session, Card card, string lobbyCode)
        {
            bool skipNext = false;
            switch (card.Value)
            {
                case CardValue.Skip:
                    skipNext = true;
                    break;
                case CardValue.Reverse:
                    session.ReverseDirection();
                    if (session.Players.Count == 2) skipNext = true;
                    break;
                case CardValue.DrawTwo:
                    ExecuteDrawForNextPlayer(session, 2, lobbyCode);
                    skipNext = true;
                    break;
                case CardValue.WildDrawFour:
                    ExecuteDrawForNextPlayer(session, 4, lobbyCode);
                    skipNext = true;
                    break;
                case CardValue.WildDrawTen:
                    ExecuteDrawForNextPlayer(session, 10, lobbyCode);
                    skipNext = true;
                    break;
                case CardValue.WildDrawSkipReverseFour:
                    session.ReverseDirection();
                    ExecuteDrawForNextPlayer(session, 4, lobbyCode);
                    skipNext = true;
                    break;
            }
            return skipNext;
        }

        private void NotifyPlay(string lobbyCode, string nickname, Card card)
        {
            _sessionHelper.BroadcastToGame(lobbyCode, cb => cb.CardPlayed(nickname, card));
            Logger.Log($"[GAME] {nickname} played {card} in lobby {lobbyCode}");
        }

        private bool ValidatePlay(GameSession session, GamePlayerData player, Card card)
        {
            if (player == null) return false;
            if (session.GetCurrentPlayer().Nickname != player.Nickname) return false;
            if (card == null) return false;

            return IsValidMove(card, session);
        }

        private void ProcessCardMove(GameSession session, Card card, PlayCardContext context)
        {
            var player = session.GetPlayer(context.Nickname);
            player.Hand.Remove(card);

            if (IsWildCard(card.Value))
            {
                if (context.SelectedColorId.HasValue && Enum.IsDefined(typeof(CardColor), context.SelectedColorId.Value))
                {
                    session.CurrentActiveColor = (CardColor)context.SelectedColorId.Value;
                }
                else
                {
                    session.CurrentActiveColor = CardColor.Red; 
                }
            }
            else
            {
                session.CurrentActiveColor = card.Color;
            }

            session.Deck.AddToDiscardPile(card);
        }

        private void AdvanceTurnLogic(GameSession session, Card card, string lobbyCode)
        {
            bool skipNext = ApplyCardEffects(session, card, lobbyCode);

            session.NextTurn();

            if (skipNext)
            {
                session.NextTurn();
            }

            _sessionHelper.BroadcastToGame(lobbyCode, cb => cb.TurnChanged(session.GetCurrentPlayer().Nickname));
        }

        private void ExecuteDrawForNextPlayer(GameSession session, int count, string lobbyCode)
        {
            int victimIndex;
            if (session.IsClockwise)
            {
                victimIndex = (session.CurrentTurnIndex + 1) % session.Players.Count;
            }
            else
            {
                victimIndex = (session.CurrentTurnIndex - 1 + session.Players.Count) % session.Players.Count;
            }

            var victim = session.Players[victimIndex];
            var drawnCards = session.Deck.DrawCards(count);
            victim.Hand.AddRange(drawnCards);
            _sessionHelper.SendToPlayer(lobbyCode, victim.Nickname, cb => cb.ReceiveCards(drawnCards));

            for (int i = 0; i < count; i++)
            {
                _sessionHelper.BroadcastToGame(lobbyCode, cb => cb.CardDrawn(victim.Nickname));
            }
        }

        private bool IsWildCard(CardValue value)
        {
            return value == CardValue.Wild ||
                   value == CardValue.WildDrawFour ||
                   value == CardValue.WildDrawTen ||
                   value == CardValue.WildDrawSkipReverseFour;
        }

        private void HandleWinCondition(GameSession session, GamePlayerData winner)
        {
            var results = new List<ResultData>();
            _sessionHelper.BroadcastToGame(session.LobbyCode, cb => cb.MatchEnded(results));
            _sessionHelper.RemoveGame(session.LobbyCode);
        }

        public Task DrawCardAsync(string lobbyCode, string nickname)
        {
            var session = _sessionHelper.GetGame(lobbyCode);
            if (session == null) return Task.CompletedTask;

            lock (session.GameLock)
            {
                if (session.GetCurrentPlayer().Nickname != nickname)
                {
                    return Task.CompletedTask;
                }

                try
                {
                    var card = session.Deck.DrawCard();
                    var player = session.GetPlayer(nickname);
                    player.Hand.Add(card);

                    _sessionHelper.SendToPlayer(lobbyCode, nickname, cb => cb.ReceiveCards(new List<Card> { card }));
                    _sessionHelper.BroadcastToGame(lobbyCode, cb => cb.CardDrawn(nickname));
                }
                catch (InvalidOperationException)
                {
                    Logger.Error($"[GAME] Deck exhausted in {lobbyCode}");
                }
                catch (CommunicationException commEx)
                {
                    Logger.Warn($"[GAME] Failed communication for deck {commEx.Message}");
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Warn($"[GAME] Timeout resolving deck {timeEx.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[GAME] Error in SayUno", ex);
                }
            }
            return Task.CompletedTask;
        }
        public Task SayUnoAsync(string lobbyCode, string nickname)
        {
            try
            {
                var session = _sessionHelper.GetGame(lobbyCode);
                if (session == null)
                {
                    return Task.CompletedTask;
                }

                lock (session.GameLock)
                {
                    var player = session.GetPlayer(nickname);
                    if (player != null && player.Hand.Count == 1)
                    {
                        player.HasSaidUno = true;
                        Logger.Log($"[GAME] {nickname} shouted UNO!");
                    }
                }
            }
            catch (InvalidOperationException)
            {
                Logger.Error($"[GAME] Could not say UNO in {lobbyCode}");
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed communication saying UNO {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout saying UNO {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error in SayUno", ex);
            }
            return Task.CompletedTask;
        }

        private async Task HandleTurnExpired(string lobbyCode, string nickname)
        {
            try
            {
                Logger.Log($"[GAME] Time expired for {nickname} in {lobbyCode}. Applying penalty.");

                var session = _sessionHelper.GetGame(lobbyCode);
                if (session == null) return;

                await DrawCardAsync(lobbyCode, nickname);

                lock (session.GameLock)
                {
                    if (session.GetCurrentPlayer().Nickname == nickname)
                    {
                        session.NextTurn();
                        _sessionHelper.BroadcastToGame(lobbyCode, cb => cb.TurnChanged(session.GetCurrentPlayer().Nickname));
                    }
                }
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed handling turn expiration {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout handling turn expiration  {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error handling turn expiration", ex);
            }
        }

        private void SendInitialStateToPlayer(IGameplayCallback callback, GameSession session, GamePlayerData player)
        {
            try
            {
                var allNicknames = session.Players.Select(p => p.Nickname).ToList();
                callback.ReceivePlayerList(allNicknames);
                callback.ReceiveInitialHand(player.Hand);

                var topCard = session.Deck.PeekTopCard();
                if (topCard != null)
                {
                    callback.CardPlayed("System", topCard);
                }

                var currentTurnPlayer = session.GetCurrentPlayer();
                callback.TurnChanged(currentTurnPlayer.Nickname);
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed to send initial state (Communication): {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout sending initial state: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Logic error mapping/sending state", ex);
            }
        }
    }
}