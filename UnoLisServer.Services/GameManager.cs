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
                var playersData = await CreateInitialPlayersDataAsync(playerNicknames);

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
                var session = _sessionHelper.GetGame(lobbyCode);
                if (session == GameSession.Empty)
                {
                    Logger.Warn($"[GAME] Session {lobbyCode} not found during connection attempt");
                    return;
                }

                var callback = OperationContext.Current.GetCallbackChannel<IGameplayCallback>();
                bool playerExists = RegisterPlayerCallback(session, nickname, callback);

                if (!playerExists)
                {
                    Logger.Warn($"[GAME] Player {nickname} not found in session {lobbyCode}");
                    return;
                }

                SendInitialStateAsync(session, nickname, callback);
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

        private async Task<List<GamePlayerData>> CreateInitialPlayersDataAsync(List<string> nicknames)
        {
            var tasks = nicknames.Select(async nick =>
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

            return (await Task.WhenAll(tasks)).ToList();
        }

        private bool RegisterPlayerCallback(GameSession session, string nickname, IGameplayCallback callback)
        {
            lock (session.GameLock)
            {
                var player = session.GetPlayer(nickname);
                if (player == null)
                {
                    return false;
                }

                _sessionHelper.UpdateCallback(session.LobbyCode, nickname, callback);
                return true;
            }
        }

        private void SendInitialStateAsync(GameSession session, string nickname, IGameplayCallback callback)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500);

                    lock (session.GameLock)
                    {
                        var player = session.GetPlayer(nickname);
                        if (player != null)
                        {
                            SendInitialStateToPlayer(callback, session, player);
                        }
                    }
                }
                catch (CommunicationException commEx)
                {
                    Logger.Warn($"[GAME-ASYNC] Failed sending initial state (Communication) to {nickname}: {commEx.Message}");
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Warn($"[GAME-ASYNC] Timeout sending initial state to {nickname}: {timeEx.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[GAME-ASYNC] Error sending initial state to {nickname}", ex);
                }
            });
        }

        public void DisconnectPlayer(string lobbyCode, string nickname)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            try
            {
                HandlePlayerLeft(lobbyCode, nickname);
                Logger.Log($"[GAME] Player disconnected handled for {lobbyCode}");
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

        private void HandlePlayerLeft(string lobbyCode, string nickname)
        {
            var session = _sessionHelper.GetGame(lobbyCode);
            if (session == GameSession.Empty)
            {
                return;
            }

            lock (session.GameLock)
            {
                var playerToRemove = session.GetPlayer(nickname);

                if (playerToRemove == null)
                {
                    return;
                }

                session.Players.Remove(playerToRemove);
                Logger.Log($"[GAME] Removed Player from session logic.");

                if (session.Players.Count < MinPlayersToContinue)
                {
                    EndGameByDefault(session);
                }
                else
                {
                    ContinueGameAfterDisconnect(session, lobbyCode);
                }
            }
        }

        private void EndGameByDefault(GameSession session)
        {
            var winner = session.Players.FirstOrDefault();

            if (winner != null)
            {
                _sessionHelper.BroadcastToGame(session.LobbyCode,
                    callback => callback.GameMessage($"Everyone left! {winner.Nickname} wins by default"));

                Task.Run(async () => await SaveDefaultWinAsync(session, winner));
            }
            else
            {
                _sessionHelper.RemoveGame(session.LobbyCode);
            }
        }

        private async Task SaveDefaultWinAsync(GameSession session, GamePlayerData winner)
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
        }

        private void ContinueGameAfterDisconnect(GameSession session, string lobbyCode)
        {
            if (session.CurrentTurnIndex >= session.Players.Count)
            {
                session.CurrentTurnIndex = 0;
            }

            _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.GameMessage("Player left!"));

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

        public async Task PlayCardAsync(PlayCardContext context)
        {
            try
            {
                var session = _sessionHelper.GetGame(context.LobbyCode);
                if (session == GameSession.Empty) return;

                bool isUnoRisk = false;
                Card cardPlayed = null;

                lock (session.GameLock)
                {
                    var player = session.GetPlayer(context.Nickname);

                    if (!ValidateTurn(session, player))
                    {
                        return;
                    }

                    cardPlayed = player.Hand.FirstOrDefault(card => card.Id == context.CardId);
                    if (!ValidatePlay(session, player, cardPlayed))
                    {
                        Logger.Warn($"[GAME] Invalid play attempt by {context.Nickname} in {context.LobbyCode}");
                        return;
                    }

                    ApplyCardMove(session, player, cardPlayed, context);
                    NotifyPlay(session.LobbyCode, context.Nickname, cardPlayed);

                    if (player.Hand.Count == 0)
                    {
                        _ = HandleWinCondition(session, player);
                        return;
                    }

                    if (IsUnoPenaltyApplicable(player))
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
                        Logger.Warn($"[GAME] UNO Risk check aborted. Player left.");
                        AdvanceTurnLogic(session, cardPlayed, session.LobbyCode);
                        return;
                    }

                    if (!player.HasSaidUno)
                    {
                        ApplyUnoPenalty(session, player);
                    }

                    AdvanceTurnLogic(session, cardPlayed, session.LobbyCode);
                }
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Communication error processing UNO risk: {commEx.Message}");
                SafeAdvanceTurn(session, cardPlayed);
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout processing UNO risk: {timeEx.Message}");
                SafeAdvanceTurn(session, cardPlayed);
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error processing UNO risk", ex);
                SafeAdvanceTurn(session, cardPlayed);
            }
        }

        private void AdvanceTurnLogic(GameSession session, Card card, string lobbyCode)
        {
            bool skipNext = ApplyCardEffects(session, card);

            session.NextTurn();
            if (skipNext)
            {
                session.NextTurn();
            }

            BroadcastTurnChange(session, lobbyCode);
        }

        private void ApplyCardMove(GameSession session, GamePlayerData player, Card card, PlayCardContext context)
        {
            player.Hand.Remove(card);
            session.Deck.AddToDiscardPile(card);

            if (IsWildCard(card.Value))
            {
                HandleWildCardColor(session, context);
            }
            else
            {
                session.CurrentActiveColor = card.Color;
            }
        }

        private void HandleWildCardColor(GameSession session, PlayCardContext context)
        {
            var newColor = CardColor.Red;

            if (context.SelectedColorId.HasValue && Enum.IsDefined(typeof(CardColor), context.SelectedColorId.Value))
            {
                newColor = (CardColor)context.SelectedColorId.Value;
            }
            else
            {
                Logger.Warn($"[GAME] Invalid/Missing color selection. Defaulting to Red.");
            }

            session.CurrentActiveColor = newColor;

            string msg = $"Color has changed to {newColor}";
            BroadcastGameMessage(session.LobbyCode, msg);
        }

        private bool ValidateTurn(GameSession session, GamePlayerData player)
        {
            return session.GetCurrentPlayer().Nickname == player.Nickname;
        }

        private bool IsUnoPenaltyApplicable(GamePlayerData player)
        {
            return player.Hand.Count == 1 && !player.HasSaidUno;
        }

        private void ApplyUnoPenalty(GameSession session, GamePlayerData player)
        {
            var penaltyCards = session.Deck.DrawCards(PenaltyCardCount);
            player.Hand.AddRange(penaltyCards);

            _sessionHelper.SendToPlayer(session.LobbyCode, player.Nickname,
                callback => callback.ReceiveCards(penaltyCards));

            BroadcastGameMessage(session.LobbyCode, $"{player.Nickname} forgot to shout UNO (+2 cards)");
        }

        private void SafeAdvanceTurn(GameSession session, Card cardPlayed)
        {
            lock (session.GameLock)
            {
                AdvanceTurnLogic(session, cardPlayed, session.LobbyCode);
            }
        }

        private void NotifyPlay(string lobbyCode, string nickname, Card card)
        {
            try
            {
                var session = _sessionHelper.GetGame(lobbyCode);
                if (session == GameSession.Empty) return;

                var player = session.GetPlayer(nickname);
                if (player == null) return;

                _sessionHelper.BroadcastToGame(lobbyCode, callback =>
                callback.CardPlayed(nickname, card, player.Hand.Count));
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed notifying play: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout notifying play: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error notifying play", ex);
            }
        }

        private void BroadcastTurnChange(GameSession session, string lobbyCode)
        {
            try
            {
                var nextPlayer = session.GetCurrentPlayer();
                if (nextPlayer != null)
                {
                    _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.TurnChanged(nextPlayer.Nickname));
                }
                else
                {
                    Logger.Error($"[CRITICAL] No player found for next turn in {lobbyCode}");
                }
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME-UI] Failed broadcast TurnChanged: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME-UI] Timeout broadcast TurnChanged: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME-UI] Error broadcast TurnChanged", ex);
            }
        }

        private void BroadcastGameMessage(string lobbyCode, string message)
        {
            try
            {
                _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.GameMessage(message));
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed broadcasting message: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout broadcasting message: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error broadcasting message", ex);
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

        public void UseItem(ItemUsageContext context)
        {
            if (IsInvalidItemContext(context)) return;

            var session = _sessionHelper.GetGame(context.LobbyCode);
            if (session == GameSession.Empty) return;

            lock (session.GameLock)
            {
                var player = session.GetPlayer(context.ActorNickname);

                if (!CanUseItem(session, player, context.ItemType)) return;

                if (context.ItemType == ItemType.SwapHands)
                {
                    ExecuteSwapHands(session, player, context.TargetNickname);
                }
                else
                {
                    Logger.Warn($"[GAME] Item logic not implemented for {context.ItemType}");
                }
            }
        }

        private bool IsInvalidItemContext(ItemUsageContext context)
        {
            return context == null ||
                   string.IsNullOrWhiteSpace(context.LobbyCode) ||
                   string.IsNullOrWhiteSpace(context.ActorNickname);
        }

        private bool CanUseItem(GameSession session, GamePlayerData player, ItemType itemType)
        {
            if (player == null || session.GetCurrentPlayer().Nickname != player.Nickname)
            {
                Logger.Warn($"[GAME] Player tried to use item out of turn or doesn't exist.");
                return false;
            }

            if (!player.Items.ContainsKey(itemType) || player.Items[itemType] <= 0)
            {
                Logger.Warn($"[GAME] Player tried to use {itemType} but has quantity 0.");
                return false;
            }
            return true;
        }

        private void ExecuteSwapHands(GameSession session, GamePlayerData player, string targetNickname)
        {
            var targetPlayer = session.GetPlayer(targetNickname);
            if (targetPlayer == null || targetPlayer.Nickname == player.Nickname)
            {
                return;
            }

            var tempHand = player.Hand;
            player.Hand = targetPlayer.Hand;
            targetPlayer.Hand = tempHand;

            player.Items[ItemType.SwapHands]--;

            _sessionHelper.SendToPlayer(session.LobbyCode, player.Nickname,
                callback => callback.ReceiveInitialHand(player.Hand));
            _sessionHelper.SendToPlayer(session.LobbyCode, targetPlayer.Nickname,
                callback => callback.ReceiveInitialHand(targetPlayer.Hand));

            BroadcastHandCounts(session);
            BroadcastGameMessage(session.LobbyCode, $"Player switched hands 🔄");
        }

        private void BroadcastHandCounts(GameSession session)
        {
            foreach (var player in session.Players)
            {
                _sessionHelper.BroadcastToGame(session.LobbyCode,
                    callback => callback.CardDrawn(player.Nickname, player.Hand.Count));
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

                if (player == null)
                {
                    return DefaultAvatar;
                }

                int? selectedAvatarId = player.SelectedAvatar_Avatar_idAvatar;

                if (selectedAvatarId == null)
                {
                    return DefaultAvatar;
                }

                var unlockedEntry = player.AvatarsUnlocked
                    .FirstOrDefault(au => au.Avatar_idAvatar == selectedAvatarId);

                if (unlockedEntry?.Avatar != null)
                {
                    return unlockedEntry.Avatar.avatarName;
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

        private bool ApplyCardEffects(GameSession session, Card card)
        {
            if (session == null || card == null) return false;

            bool skipNext = false;

            switch (card.Value)
            {
                case CardValue.Skip:
                    skipNext = true;
                    break;
                case CardValue.Reverse:
                    HandleReverseCard(session, ref skipNext);
                    break;
                case CardValue.DrawTwo:
                    ExecuteDrawAndSkip(session, 2, ref skipNext);
                    break;
                case CardValue.WildDrawFour:
                    ExecuteDrawAndSkip(session, 4, ref skipNext);
                    break;
                case CardValue.WildDrawTen:
                    ExecuteDrawAndSkip(session, 10, ref skipNext);
                    break;
                case CardValue.WildDrawSkipReverseFour:
                    session.ReverseDirection();
                    ExecuteDrawAndSkip(session, 4, ref skipNext);
                    break;
            }
            return skipNext;
        }

        private void ExecuteDrawAndSkip(GameSession session, int count, ref bool skipNext)
        {
            ExecuteDrawForNextPlayer(session, count, session.LobbyCode);
            skipNext = true;
        }

        private void HandleReverseCard(GameSession session, ref bool skipNext)
        {
            session.ReverseDirection();
            if (session.Players.Count == 2)
            {
                skipNext = true;
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

        private bool IsWildCard(CardValue value)
        {
            return value == CardValue.Wild ||
                   value == CardValue.WildDrawFour ||
                   value == CardValue.WildDrawTen ||
                   value == CardValue.WildDrawSkipReverseFour;
        }

        public void ExecuteDrawForNextPlayer(GameSession session, int count, string lobbyCode)
        {
            try
            {
                var victim = GetNextPlayer(session);

                var drawnCards = session.Deck.DrawCards(count);
                victim.Hand.AddRange(drawnCards);
                victim.HasSaidUno = false;

                NotifyVictimDrawSafe(lobbyCode, victim.Nickname, drawnCards);
                BroadcastDrawAnimationSafe(lobbyCode, victim.Nickname, victim.Hand.Count, count);
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Communication error executing draw logic for next player in {lobbyCode}: " +
                    $"{commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout executing draw logic for next player in {lobbyCode}: {timeEx.Message}");
            }
            catch (InvalidOperationException)
            {
                Logger.Error($"[GAME] Deck exhausted in {lobbyCode} during draw execution.");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error executing draw logic for next player in {lobbyCode}", ex);
            }
        }

        private GamePlayerData GetNextPlayer(GameSession session)
        {
            int step = 1;
            int victimIndex;

            if (session.IsClockwise)
            {
                victimIndex = (session.CurrentTurnIndex + step) % session.Players.Count;
            }
            else
            {
                victimIndex = (session.CurrentTurnIndex - step + session.Players.Count) % session.Players.Count;
            }

            return session.Players[victimIndex];
        }

        private void NotifyVictimDrawSafe(string lobbyCode, string nickname, List<Card> drawnCards)
        {
            try
            {
                _sessionHelper.SendToPlayer(lobbyCode, nickname,
                    callback => callback.ReceiveCards(drawnCards));
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed sending private cards to {nickname}: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout sending private cards to {nickname}: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Unexpected error sending private cards to {nickname}", ex);
            }
        }

        private void BroadcastDrawAnimationSafe(string lobbyCode, string nickname, int totalHandCount, int drawCount)
        {
            try
            {
                for (int i = 0; i < drawCount; i++)
                {
                    _sessionHelper.BroadcastToGame(lobbyCode,
                        callback => callback.CardDrawn(nickname, totalHandCount));
                }
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed broadcasting draw animation for {nickname}: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout broadcasting draw animation for {nickname}: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Unexpected error broadcasting draw animation for {nickname}", ex);
            }
        }

        private async Task HandleWinCondition(GameSession session, GamePlayerData winner)
        {
            if (session == null || session == GameSession.Empty)
            {
                return;
            }

            try
            {
                var results = BuildMatchResults(session, winner);

                await UpdateAllPlayersStatsAsync(results);
                BroadcastMatchEndSafe(session.LobbyCode, results);

                _sessionHelper.RemoveGame(session.LobbyCode);
                Logger.Log($"[GAME] Match {session.LobbyCode} ended successfully.");
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Communication error handling win condition for {session.LobbyCode}: " +
                    $"{commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout handling win condition for {session.LobbyCode}: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Critical error handling win condition for {session.LobbyCode}", ex);
            }
        }

        private List<ResultData> BuildMatchResults(GameSession session, GamePlayerData winner)
        {
            var rankedPlayers = GetRankedPlayers(session, winner);
            var results = new List<ResultData>();

            for (int rank = 0; rank < rankedPlayers.Count; rank++)
            {
                var player = rankedPlayers[rank];
                int points = CalculatePoints(rankedPlayers.Count, rank);

                results.Add(new ResultData
                {
                    Nickname = player.Nickname,
                    Rank = rank + 1,
                    Score = points,
                    AvatarName = player.AvatarName
                });
            }
            return results;
        }

        private List<GamePlayerData> GetRankedPlayers(GameSession session, GamePlayerData winner)
        {
            if (session == null || winner == null)
            {
                return new List<GamePlayerData>();
            }

            return session.Players
                .OrderBy(player => player.Nickname == winner.Nickname ? 0 : 1)
                .ThenBy(player => player.Hand.Count)
                .ToList();
        }

        private int CalculatePoints(int totalPlayers, int rankIndex)
        {
            var scoreTable = new Dictionary<int, int[]>
            {
                { 2, new[] { 100, 0 } },
                { 3, new[] { 300, 100, 0 } },
                { 4, new[] { 500, 300, 200, 50 } }
            };

            if (scoreTable.TryGetValue(totalPlayers, out var scores) && rankIndex < scores.Length)
            {
                return scores[rankIndex];
            }

            return 0;
        }

        private async Task UpdateAllPlayersStatsAsync(List<ResultData> results)
        {
            foreach (var result in results)
            {
                if (UserHelper.IsGuest(result.Nickname))
                {
                    continue;
                }

                try
                {
                    bool isWinner = (result.Rank == 1);
                    await _playerRepository.UpdateMatchResultAsync(result.Nickname, isWinner, result.Score);
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Warn($"[GAME] Timeout updating stats for Player: {timeEx.Message}");
                }
                catch (Exception ex) when (ex.Message == "DataStore_Unavailable" || ex.Message == "Player_Not_Found")
                {
                    Logger.Warn($"[GAME] Could not update stats (DB Unavailable).");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[GAME] Error updating stats", ex);
                }
            }
        }

        private void BroadcastMatchEndSafe(string lobbyCode, List<ResultData> results)
        {
            try
            {
                _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.MatchEnded(results));
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed broadcasting match end: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout broadcasting match end: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error broadcasting match end", ex);
            }
        }

        public Task DrawCardAsync(string lobbyCode, string nickname)
        {
            if (IsInvalidInput(lobbyCode, nickname))
            {
                return Task.CompletedTask;
            }

            try
            {
                var session = _sessionHelper.GetGame(lobbyCode);
                if (session == GameSession.Empty)
                {
                    return Task.CompletedTask;
                }

                lock (session.GameLock)
                {
                    if (!ValidateTurnDraw(session, nickname))
                    {
                        return Task.CompletedTask;
                    }

                    var drawnCard = ExecuteDrawStateChange(session, nickname);

                    if (drawnCard != null)
                    {
                        var player = session.GetPlayer(nickname);
                        NotifyDrawSafe(lobbyCode, nickname, drawnCard, player.Hand.Count);
                    }
                }
            }
            catch (InvalidOperationException)
            {
                Logger.Error($"[GAME] Deck exhausted in {lobbyCode}. Cannot draw.");
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
                Logger.Error($"[GAME] Critical error drawing card in {lobbyCode}", ex);
            }

            return Task.CompletedTask;
        }

        private bool ValidateTurnDraw(GameSession session, string nickname)
        {
            if (session.GetCurrentPlayer().Nickname != nickname)
            {
                Logger.Warn($"[GAME] Turn violation: Player {nickname} tried to draw out of turn.");
                return false;
            }
            return true;
        }

        private Card ExecuteDrawStateChange(GameSession session, string nickname)
        {
                var card = session.Deck.DrawCard();
                var player = session.GetPlayer(nickname);

                player.Hand.Add(card);
                player.HasSaidUno = false;

                return card;
        }

        private void NotifyDrawSafe(string lobbyCode, string nickname, Card card, int currentHandCount)
        {
            try
            {
                _sessionHelper.SendToPlayer(lobbyCode, nickname,
                    callback => callback.ReceiveCards(new List<Card> { card }));

                _sessionHelper.BroadcastToGame(lobbyCode,
                    callback => callback.CardDrawn(nickname, currentHandCount));
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed communication during draw notification: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout during draw notification: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Unexpected error notifying draw for {nickname}", ex);
            }
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

                        BroadcastShoutedUnoSafe(lobbyCode, nickname);
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
                Logger.Log($"[GAME] Time expired for Player in {lobbyCode}. Applying penalty.");

                var session = _sessionHelper.GetGame(lobbyCode);
                if (session == null || session == GameSession.Empty)
                {
                    return;
                }

                await DrawCardAsync(lobbyCode, nickname);

                lock (session.GameLock)
                {
                    if (session.GetCurrentPlayer().Nickname == nickname)
                    {
                        session.NextTurn();
                        BroadcastTurnChange(session, lobbyCode);
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

        private void BroadcastShoutedUnoSafe(string lobbyCode, string nickname)
        {
            try
            {
                _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.PlayerShoutedUno(nickname));
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME] Failed broadcasting UNO shout: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAME] Timeout broadcasting UNO shout: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"[GAME] Failed broadcasting UNO shout: {ex.Message}");
            }
        }

        private bool IsInvalidInput(string lobbyCode, string nickname)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(nickname))
            {
                Logger.Warn("[GAMEPLAY] Invalid parameters received (null or empty).");
                return true;
            }
            return false;
        }
    }
}