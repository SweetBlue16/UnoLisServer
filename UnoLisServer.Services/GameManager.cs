using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Enums;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services.GameLogic;
using UnoLisServer.Services.GameLogic.Models;
using UnoLisServer.Services.Helpers;
using UnoLisServer.Services.ManagerInterfaces;

namespace UnoLisServer.Services
{
    /// <summary>
    /// Class for managing game logic and sessions during gameplay
    /// </summary>
    public class GameManager : IGameManager
    {
        private const int UnoPenaltyDelayMs = 3000;
        private const int PenaltyCardCount = 2;
        private const int MinPlayersToContinue = 2;
        private readonly GameSessionHelper _sessionHelper;
        private readonly IPlayerRepository _playerRepository;

        public GameManager() : this(GameSessionHelper.Instance, new PlayerRepository()) 
        {
        }

        public GameManager(GameSessionHelper sessionHelper, IPlayerRepository playerRepo)
        {
            _sessionHelper = sessionHelper ?? throw new ArgumentNullException(nameof(sessionHelper));
            _playerRepository = playerRepo ?? throw new ArgumentNullException(nameof(playerRepo));

            _sessionHelper.OnPlayerDisconnected -= HandlePlayerLeft;
            _sessionHelper.OnPlayerDisconnected += HandlePlayerLeft;
        }

        public async Task<bool> InitializeGameAsync(string lobbyCode, List<string> playerNicknames)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || playerNicknames == null || !playerNicknames.Any())
            {
                return false;
            }

            try
            {
                var playerTasks = playerNicknames.Select(async nick =>
                {
                string avatar = await GetAvatarAsync(nick);
                    return new GamePlayerData
                    {
                        Nickname = nick,
                        AvatarName = avatar,
                        Hand = new List<Card>(),
                        Items = CreateDefaultInventory()
                    };
                });

                var playersData = (await Task.WhenAll(playerTasks)).ToList();
                var session = new GameSession(lobbyCode, playersData);
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
                Logger.Warn($"[GAME] ConnectPlayer called without OperationContext");
                return;
            }

            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<IGameplayCallback>();
                var session = _sessionHelper.GetGame(lobbyCode);

                if (session != GameSession.Empty)
                {
                    lock (session.GameLock)
                    {
                        var player = session.GetPlayer(nickname);

                        if (player == null || string.IsNullOrWhiteSpace(player.Nickname))
                        {
                            Logger.Warn($"[GAME] Player not found in session logic {lobbyCode}");
                            return;
                        }

                        _sessionHelper.UpdateCallback(lobbyCode, nickname, callback);
                    }

                    Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(500);
                            lock (session.GameLock)
                            {
                                var playerRef = session.GetPlayer(nickname);
                                if (playerRef != null || !string.IsNullOrWhiteSpace(playerRef.Nickname))
                                {
                                    SendInitialStateToPlayer(callback, session, playerRef);
                                }
                            }
                        }
                        catch (InvalidOperationException invOpEx)
                        {
                            Logger.Warn($"[GAME-ASYNC] Invalid operation sending initial state: {invOpEx.Message}");
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"[GAME-ASYNC] Error sending initial state to {nickname}", ex);
                        }
                    });
                }
                else
                {
                    Logger.Warn($"[GAME] Session {lobbyCode} not found during connection attempt");
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
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(nickname)) return;

            try
            {
                HandlePlayerLeft(lobbyCode, nickname);
                Logger.Log($"[GAME] Player {nickname} disconnected handled for {lobbyCode}");
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed handling disconnection (Communication) in {lobbyCode}: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout handling disconnection in {lobbyCode}: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error handling disconnection for {nickname} in {lobbyCode}", ex);
            }
        }

        public async Task PlayCardAsync(PlayCardContext context)
        {
            try
            {
                var session = _sessionHelper.GetGame(context.LobbyCode);
                if (session == null || session == GameSession.Empty) return;

                bool isUnoRisk = false;
                Card cardPlayed = null;

                lock (session.GameLock)
                {
                    var player = session.GetPlayer(context.Nickname);

                    if (session.GetCurrentPlayer().Nickname != player.Nickname)
                    {
                        return;
                    }

                    cardPlayed = player.Hand.FirstOrDefault(c => c.Id == context.CardId);

                    if (!ValidatePlay(session, player, cardPlayed))
                    {
                        Logger.Warn($"[GAME] Invalid play attempt by {context.Nickname} in {context.LobbyCode}");
                        return;
                    }

                    ProcessCardMove(session, cardPlayed, context);
                    NotifyPlay(context.LobbyCode, context.Nickname, cardPlayed);

                    int emptyHand = 0;
                    if (player.Hand.Count == emptyHand)
                    {
                        _ = HandleWinCondition(session, player);
                        return;
                    }

                    if (player.Hand.Count == 1 && !player.HasSaidUno)
                    {
                        isUnoRisk = true;
                    }
                    else
                    {
                        AdvanceTurnLogic(session, cardPlayed, context.LobbyCode);
                    }
                } 

                if (isUnoRisk)
                {
                    await HandleUnoRiskAsync(session, context.Nickname, cardPlayed);
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
        }

        private async Task HandleUnoRiskAsync(GameSession session, string nickname, Card cardPlayed)
        {
            try
            {
                await Task.Delay(UnoPenaltyDelayMs);

                lock (session.GameLock)
                {
                    var player = session.GetPlayer(nickname);
                    if (player == null || string.IsNullOrWhiteSpace(player.Nickname))
                    {
                        Logger.Warn($"[GAME] UNO Risk check aborted. Player left the session.");
                        AdvanceTurnLogic(session, cardPlayed, session.LobbyCode);
                        return;
                    }

                    if (!player.HasSaidUno)
                    {
                        var penaltyCards = session.Deck.DrawCards(PenaltyCardCount);
                        player.Hand.AddRange(penaltyCards);

                        _sessionHelper.SendToPlayer(session.LobbyCode, player.Nickname, callback => callback.ReceiveCards(penaltyCards));
                        _sessionHelper.BroadcastToGame(session.LobbyCode, callback => callback.GameMessage($"Player forgot olvidó gritar UNO (+2 cartas)"));
                    }

                    AdvanceTurnLogic(session, cardPlayed, session.LobbyCode);
                }
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed processing UNO risk (Communication): {commEx.Message}");
                lock (session.GameLock)
                {
                    AdvanceTurnLogic(session, cardPlayed, session.LobbyCode);
                }
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout processing UNO risk: {timeEx.Message}");
                lock (session.GameLock)
                {
                    AdvanceTurnLogic(session, cardPlayed, session.LobbyCode);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error processing UNO risk", ex);
                lock (session.GameLock)
                {
                    AdvanceTurnLogic(session, cardPlayed, session.LobbyCode);
                }
            }
        }

        private void HandlePlayerLeft(string lobbyCode, string nickname)
        {
            try
            {
                var session = _sessionHelper.GetGame(lobbyCode);
                if (session == null || session == GameSession.Empty)
                {
                    return;
                }

                lock (session.GameLock)
                {
                    var playerToRemove = session.GetPlayer(nickname);
                    if (playerToRemove == null || string.IsNullOrWhiteSpace(playerToRemove.Nickname))
                    {
                        return;
                    }

                    session.Players.Remove(playerToRemove);
                    Logger.Log($"[GAME] Removed Player from session logic.");

                    if (session.Players.Count < MinPlayersToContinue)
                    {
                        var winner = session.Players.FirstOrDefault();
                        if (winner != null)
                        {
                            _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.GameMessage($"Everyone left" +
                                $"! {winner.Nickname} wins by default."));

                            Task.Run(async () =>
                            {
                                try
                                {
                                    await HandleWinCondition(session, winner);
                                }
                                catch (CommunicationException commEx)
                                {
                                    Logger.Warn($"[GAME] Error saving default win (Communication): {commEx.Message}");
                                }
                                catch (TimeoutException timeEx)
                                {
                                    Logger.Warn($"[GAME] Timeout saving default win: {timeEx.Message}");
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error($"[GAME] Error saving default win", ex);
                                }
                            });
                        }
                        else
                        {
                            _sessionHelper.RemoveGame(lobbyCode);
                        }

                        return;
                    }

                    if (session.CurrentTurnIndex >= session.Players.Count)
                    {
                        session.CurrentTurnIndex = 0;
                    }

                    _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.GameMessage($"Player " +
                        $"disconnected."));

                    var activePlayers = session.Players.Select(player => new GamePlayer
                    {
                        Nickname = player.Nickname,
                        CardCount = player.Hand.Count,
                        AvatarName = player.AvatarName
                    }).ToList();

                    _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.ReceivePlayerList(activePlayers));

                    var currentPlayer = session.GetCurrentPlayer();
                    if (currentPlayer != null)
                    {
                        _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.TurnChanged(currentPlayer.Nickname));
                    }
                }
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed handling player left (Communication) in {lobbyCode}: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout handling player left in {lobbyCode}: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error handling player left in {lobbyCode}", ex);
            }
        }

        public void UseItem(string lobbyCode, string nickname, ItemType itemType, string targetNickname)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            var session = _sessionHelper.GetGame(lobbyCode);
            if (session == null || session == GameSession.Empty)
            {
                return;
            }

            lock (session.GameLock)
            {
                var player = session.GetPlayer(nickname);

                if (player == null || session.GetCurrentPlayer().Nickname != nickname)
                {
                    Logger.Warn($"[GAME] Player tried to use item out of turn or doesn't exist.");
                    return;
                }

                int emptyItemCount = 0;
                if (!player.Items.ContainsKey(itemType) || player.Items[itemType] <= emptyItemCount)
                {
                    Logger.Warn($"[GAME] Player tried to use {itemType} but has quantity 0.");
                    return;
                }

                if (itemType == ItemType.SwapHands)
                {
                    var targetPlayer = session.GetPlayer(targetNickname);
                    if (targetPlayer == null || targetPlayer.Nickname == nickname)
                    {
                        return;
                    }

                    var tempHand = player.Hand;
                    player.Hand = targetPlayer.Hand;
                    targetPlayer.Hand = tempHand;

                    player.Items[itemType]--;

                    _sessionHelper.SendToPlayer(lobbyCode, player.Nickname, callback => 
                    callback.ReceiveInitialHand(player.Hand));
                    _sessionHelper.SendToPlayer(lobbyCode, targetPlayer.Nickname, callback => 
                    callback.ReceiveInitialHand(targetPlayer.Hand));

                    foreach (var matchPlayer in session.Players)
                    {
                        _sessionHelper.BroadcastToGame(lobbyCode, callback => 
                        callback.CardDrawn(matchPlayer.Nickname, matchPlayer.Hand.Count));
                    }

                    _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.GameMessage($"Player " +
                        $"switched hands 🔄"));
                }
            }
        }

        private async Task<string> GetAvatarAsync(string nickname)
        {
            const string DefaultAvatar = "LogoUNO";

            if (UserHelper.IsGuest(nickname))
            {
                return DefaultAvatar;
            }

            try
            {
                var player = await _playerRepository.GetPlayerWithDetailsAsync(nickname);

                if (player?.SelectedAvatar_Avatar_idAvatar != null)
                {
                    var unlocked = player.AvatarsUnlocked
                        .FirstOrDefault(avatarsUnlocked => avatarsUnlocked.Avatar_idAvatar == 
                        player.SelectedAvatar_Avatar_idAvatar);

                    if (unlocked?.Avatar != null)
                    {
                        return unlocked.Avatar.avatarName;
                    }
                }
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Warn($"[GAME] Avatar fetch failed (DB Unavailable). Using default.");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Unexpected error fetching avatar", ex);
            }

            return DefaultAvatar;
        }

        private bool IsValidMove(Card card, GameSession session)
        {
            if (card == null || session == null)
            {
                return false;
            }

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
            if (session == null || card == null)
            {
                return false;
            }

            int pairPlayers = 2;
            int drawFour = 4;
            int drawTen = 10;
            bool skipNext = false;
            switch (card.Value)
            {
                case CardValue.Skip:
                    skipNext = true;
                    break;
                case CardValue.Reverse:
                    session.ReverseDirection();
                    if (session.Players.Count == pairPlayers)
                    {
                        skipNext = true;
                    }
                    break;
                case CardValue.DrawTwo:
                    ExecuteDrawForNextPlayer(session, pairPlayers, lobbyCode);
                    skipNext = true;
                    break;
                case CardValue.WildDrawFour:
                    ExecuteDrawForNextPlayer(session, drawFour, lobbyCode);
                    skipNext = true;
                    break;
                case CardValue.WildDrawTen:
                    ExecuteDrawForNextPlayer(session, drawTen, lobbyCode);
                    skipNext = true;
                    break;
                case CardValue.WildDrawSkipReverseFour:
                    session.ReverseDirection();
                    ExecuteDrawForNextPlayer(session, drawFour, lobbyCode);
                    skipNext = true;
                    break;
            }
            return skipNext;
        }

        private void NotifyPlay(string lobbyCode, string nickname, Card card)
        {
            try
            {
                var session = _sessionHelper.GetGame(lobbyCode);
                if (session == null || session == GameSession.Empty)
                {
                    return;
                }

                var player = session.GetPlayer(nickname);
                if (player == null || string.IsNullOrWhiteSpace(player.Nickname))
                {
                    Logger.Warn($"[GAME] NotifyPlay skipped. Player not found in session.");
                    return;
                }

                int count = player.Hand.Count;

                _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.CardPlayed(nickname, card, count));
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed notifying play (Communication): {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout notifying play: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error notifying play in {lobbyCode}", ex);
            }
        }

        private bool ValidatePlay(GameSession session, GamePlayerData player, Card card)
        {
            if (session == null || player == null || card == null)
            {
                return false;
            }

            var currentPlayer = session.GetCurrentPlayer();
            if (currentPlayer == null || currentPlayer.Nickname != player.Nickname)
            {
                return false;
            }

            return IsValidMove(card, session);
        }

        private void ProcessCardMove(GameSession session, Card card, PlayCardContext context)
        {
            var player = session.GetPlayer(context.Nickname);
            if (player == null || string.IsNullOrWhiteSpace(player.Nickname))
            {
                return;
            }

            player.Hand.Remove(card);

            if (IsWildCard(card.Value))
            {
                if (context.SelectedColorId.HasValue && Enum.IsDefined(typeof(CardColor), 
                    context.SelectedColorId.Value))
                {
                    var selectedColor = (CardColor)context.SelectedColorId.Value;
                    session.CurrentActiveColor = selectedColor;

                    try
                    {
                        string message = $"Player has changed color to {selectedColor}";
                        _sessionHelper.BroadcastToGame(session.LobbyCode, callback => callback.GameMessage(message));
                    }
                    catch (CommunicationException commEx)
                    {
                        Logger.Warn($"[GAME] Failed broadcasting color change (Communication): {commEx.Message}");
                    }
                    catch (TimeoutException timeEx)
                    {
                        Logger.Warn($"[GAME] Timeout broadcasting color change: {timeEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"[GAME] Error broadcasting color change message", ex);
                    }
                }
                else
                {
                    Logger.Warn($"[GAME] Invalid/Missing color selection. Defaulting to Red.");
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

            try
            {
                var nextPlayer = session.GetCurrentPlayer();

                if (nextPlayer != null)
                {
                    _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.TurnChanged(nextPlayer.Nickname));
                }
                else
                {
                    Logger.Error($"[CRITICAL] Turn advanced but no player found in {lobbyCode}. Game " +
                        $"state might be empty.");
                }
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME-UI] Failed to broadcast TurnChanged (Communication) in {lobbyCode}: " +
                    $"{commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME-UI] Timeout broadcasting TurnChanged in {lobbyCode}: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME-UI] Failed to broadcast TurnChanged in {lobbyCode}", ex);
            }
        }

        private Dictionary<ItemType, int> CreateDefaultInventory()
        {
            return new Dictionary<ItemType, int>
            {
                { ItemType.SwapHands, 1 }, 
                { ItemType.Shield, 1 },
                { ItemType.Thief, 1 }
            };
        }

        private void ExecuteDrawForNextPlayer(GameSession session, int count, string lobbyCode)
        {
            try
            {
                int victimIndex;
                int step = 1;
                if (session.IsClockwise)
                {
                    victimIndex = (session.CurrentTurnIndex + step) % session.Players.Count;
                }
                else
                {
                    victimIndex = (session.CurrentTurnIndex - step + session.Players.Count) % session.Players.Count;
                }

                var victim = session.Players[victimIndex];
                var drawnCards = session.Deck.DrawCards(count);

                victim.Hand.AddRange(drawnCards);
                victim.HasSaidUno = false;

                int currentHandCount = victim.Hand.Count;
                try
                {
                    _sessionHelper.SendToPlayer(lobbyCode, victim.Nickname, callback => callback.ReceiveCards(drawnCards));
                }
                catch (CommunicationException commEx)
                {
                    Logger.Warn($"[GAME] Failed sending drawn cards (Communication) to {victim.Nickname}: {commEx.Message}");
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Warn($"[GAME] Timeout sending drawn cards to {victim.Nickname}: {timeEx.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[GAME] Error sending drawn cards to {victim.Nickname}", ex);
                }
                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.CardDrawn(victim.Nickname, currentHandCount));
                    }
                }
                catch (CommunicationException commEx)
                {
                    Logger.Warn($"[GAME] Failed broadcasting drawn cards (Communication) for {victim.Nickname}: {commEx.Message}");
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Warn($"[GAME] Timeout broadcasting drawn cards for {victim.Nickname}: {timeEx.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[GAME] Error broadcasting drawn cards for {victim.Nickname}", ex);
                }
            }
            catch (InvalidOperationException)
            {
                Logger.Error($"[GAME] Deck exhausted in {lobbyCode} during draw execution.");
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed executing draw for next player (Communication): {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout executing draw for next player: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error executing draw for next player", ex);
            }
        }

        private bool IsWildCard(CardValue value)
        {
            return value == CardValue.Wild ||
                   value == CardValue.WildDrawFour ||
                   value == CardValue.WildDrawTen ||
                   value == CardValue.WildDrawSkipReverseFour;
        }

        private async Task HandleWinCondition(GameSession session, GamePlayerData winner)
        {
            if (session == null || session == GameSession.Empty)
            {
                return;
            }

            try
            {
                var rankedPlayers = GetRankedPlayers(session, winner);

                int totalPlayers = rankedPlayers.Count;
                var results = new List<ResultData>();

                for (int rank = 0; rank < totalPlayers; rank++)
                {
                    var player = rankedPlayers[rank];
                    int points = CalculatePoints(totalPlayers, rank);
                    bool isWinner = (rank == 0);

                    results.Add(new ResultData
                    {
                        Nickname = player.Nickname,
                        Rank = rank + 1,
                        Score = points,
                        AvatarName = player.AvatarName
                    });

                    if (!UserHelper.IsGuest(player.Nickname))
                    {
                        try
                        {
                            await _playerRepository.UpdateMatchResultAsync(player.Nickname, isWinner, points);
                        }
                        catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
                        {
                            Logger.Warn($"[GAME] Could not update match result for {player.Nickname} " +
                                $"(DB Unavailable).");
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"[GAME] Unexpected error updating match result for " +
                                $"{player.Nickname}", ex);
                        }
                    }
                }
                try
                {
                    _sessionHelper.BroadcastToGame(session.LobbyCode, callback => callback.MatchEnded(results));
                    _sessionHelper.RemoveGame(session.LobbyCode);
                }
                catch (CommunicationException commEx)
                {
                    Logger.Warn($"[GAME] Failed broadcasting match end (Communication): {commEx.Message}");
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Warn($"[GAME] Timeout broadcasting match end: {timeEx.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[GAME] Error broadcasting match end for {session.LobbyCode}", ex);
                }

                Logger.Log($"[GAME] Match {session.LobbyCode} ended.");
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed handling win condition (Communication): {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout handling win condition: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error handling win condition for {session.LobbyCode}", ex);
            }
        }

        private List<GamePlayerData> GetRankedPlayers(GameSession session, GamePlayerData winner)
        {
            if (session == null || winner == null)
            {
                return new List<GamePlayerData>();
            }

            var allPlayers = session.Players;

            var losers = allPlayers
                .Where(p => p.Nickname != winner.Nickname)
                .OrderBy(p => p.Hand.Count)
                .ToList();

            var rankedList = new List<GamePlayerData> { winner };
            rankedList.AddRange(losers);

            return rankedList;
        }

        private int CalculatePoints(int totalPlayers, int rankIndex)
        {
            int pairPlayers = 2;
            int trioPlayers = 3;
            int quadPlayers = 4;
            if (totalPlayers == pairPlayers)
            {
                if (rankIndex == 0)
                {
                    return 100;
                }
                return 0;
            }
            else if (totalPlayers == trioPlayers)
            {
                if (rankIndex == 0)
                {
                    return 300;
                }
                if (rankIndex == 1)
                {
                    return 100;
                }
                return 0;
            }
            else if (totalPlayers == quadPlayers) 
            {
                if (rankIndex == 0)
                {
                    return 500;
                }
                if (rankIndex == 1)
                {
                    return 300;
                }
                if (rankIndex == 2)
                {
                    return 200;
                }
                return 50;
            }

            return 0;
        }

        public Task DrawCardAsync(string lobbyCode, string nickname)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(nickname))
            {
                return Task.CompletedTask;
            }

            try
            {
                var session = _sessionHelper.GetGame(lobbyCode);
                if (session == null || session == GameSession.Empty)
                {
                    return Task.CompletedTask;
                }

                lock (session.GameLock)
                {
                    if (session.GetCurrentPlayer().Nickname != nickname)
                    {
                        Logger.Warn($"[GAME] Turn violation: Player tried to draw out of turn.");
                        return Task.CompletedTask;
                    }

                    try
                    {
                        var card = session.Deck.DrawCard();
                        var player = session.GetPlayer(nickname);
                        player.Hand.Add(card);
                        player.HasSaidUno = false;

                        int currentHandCount = player.Hand.Count;
                        _sessionHelper.SendToPlayer(lobbyCode, nickname, callback => callback.ReceiveCards(new List<Card> { card }));
                        _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.CardDrawn(nickname, currentHandCount));
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
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed drawing card (Communication): {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout drawing card: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error drawing card in {lobbyCode}", ex);
            }
            return Task.CompletedTask;
        }

        public Task SayUnoAsync(string lobbyCode, string nickname)
        {
            try
            {
                var session = _sessionHelper.GetGame(lobbyCode);
                if (session == null || session == GameSession.Empty)
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

                        string message = $"{nickname} ha gritado ¡UNO!";
                        _sessionHelper.BroadcastToGame(lobbyCode, cb => cb.PlayerShoutedUno(nickname));
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
                if (session == null || session == GameSession.Empty) return;

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
                var playersList = session.Players.Select(matchPlayer => new GamePlayer
                {
                    Nickname = matchPlayer.Nickname,
                    AvatarName = matchPlayer.AvatarName,
                    CardCount = matchPlayer.Hand.Count
                }).ToList();

                callback.ReceivePlayerList(playersList);
                callback.ReceiveInitialHand(player.Hand);

                var topCard = session.Deck.PeekTopCard();
                if (topCard != null)
                {
                    callback.CardPlayed("System", topCard, 0);
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