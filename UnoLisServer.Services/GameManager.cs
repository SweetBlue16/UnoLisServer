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
    public class GameManager : IGameManager
    {
        private readonly GameSessionHelper _sessionHelper;
        private readonly IPlayerRepository _playerRepository;

        public GameManager() : this(GameSessionHelper.Instance, new PlayerRepository()) 
        {
        }

        public GameManager(GameSessionHelper sessionHelper, IPlayerRepository playerRepo)
        {
            _sessionHelper = sessionHelper;
            _playerRepository = playerRepo;
            _sessionHelper.OnPlayerDisconnected += HandlePlayerLeft;
        }

        public async Task<bool> InitializeGameAsync(string lobbyCode, List<string> playerNicknames)
        {
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
                Logger.Warn($"[GAME] ConnectPlayer called without OperationContext for {nickname}");
                return;
            }

            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<IGameplayCallback>();
                var session = _sessionHelper.GetGame(lobbyCode);

                if (session != null)
                {
                    lock (session.GameLock)
                    {
                        var player = session.GetPlayer(nickname);

                        if (player == null)
                        {
                            Logger.Warn($"[GAME] Player {nickname} not found in session logic {lobbyCode}");
                            return;
                        }

                        _sessionHelper.UpdateCallback(lobbyCode, nickname, callback);

                        System.Threading.Thread.Sleep(500);
                        SendInitialStateToPlayer(callback, session, player);
                    }
                }
                else
                {
                    Logger.Warn($"[GAME] Session {lobbyCode} not found for ConnectPlayer");
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
            HandlePlayerLeft(lobbyCode, nickname);
        }

        public async Task PlayCardAsync(PlayCardContext context)
        {
            try
            {
                var session = _sessionHelper.GetGame(context.LobbyCode);
                if (session == null) return;

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
                await Task.Delay(3000);

                lock (session.GameLock)
                {
                    var player = session.GetPlayer(nickname);

                    if (!player.HasSaidUno)
                    {
                        Logger.Log($"[GAME] {nickname} forgot UNO! Penalizing...");

                        var penaltyCards = session.Deck.DrawCards(2);
                        player.Hand.AddRange(penaltyCards);

                        _sessionHelper.SendToPlayer(session.LobbyCode, player.Nickname, cb => cb.ReceiveCards(penaltyCards));
                        _sessionHelper.BroadcastToGame(session.LobbyCode, cb => cb.GameMessage($"{player.Nickname} olvidó gritar UNO (+2 cartas)"));
                    }
                    else
                    {
                        Logger.Log($"[GAME] {nickname} said UNO in time.");
                    }

                    AdvanceTurnLogic(session, cardPlayed, session.LobbyCode);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error processing UNO risk for {nickname}", ex);
                lock (session.GameLock)
                {
                    AdvanceTurnLogic(session, cardPlayed, session.LobbyCode);
                }
            }
        }

        private void HandlePlayerLeft(string lobbyCode, string nickname)
        {
            var session = _sessionHelper.GetGame(lobbyCode);
            if (session == null)
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
                Logger.Log($"[GAME] Removed {nickname} from session logic.");

                if (session.Players.Count < 2)
                {
                    var winner = session.Players.FirstOrDefault();
                    if (winner != null)
                    {
                        _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.GameMessage($"¡Todos se han " +
                            $"ido! {winner.Nickname} gana por default."));
                        _ = HandleWinCondition(session, winner);
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

                _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.GameMessage($"{nickname} se " +
                    $"desconectó."));

                var activePlayers = session.Players.Select(player => new GamePlayer 
                { 
                    Nickname = player.Nickname, 
                    CardCount = player.Hand.Count, 
                    AvatarName = player.AvatarName 
                }).ToList();

                _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.ReceivePlayerList(activePlayers));
                _sessionHelper.BroadcastToGame(lobbyCode, 
                    callback => callback.TurnChanged(session.GetCurrentPlayer().Nickname));
            }
        }

        public void UseItem(string lobbyCode, string nickname, ItemType itemType, string targetNickname)
        {
            var session = _sessionHelper.GetGame(lobbyCode);
            if (session == null) return;

            lock (session.GameLock)
            {
                var player = session.GetPlayer(nickname);

                if (player == null || session.GetCurrentPlayer().Nickname != nickname)
                {
                    return;
                }

                if (!player.Items.ContainsKey(itemType) || player.Items[itemType] <= 0)
                {
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

                    _sessionHelper.SendToPlayer(lobbyCode, player.Nickname, cb => cb.ReceiveInitialHand(player.Hand));
                    _sessionHelper.SendToPlayer(lobbyCode, targetPlayer.Nickname, cb => cb.ReceiveInitialHand(targetPlayer.Hand));

                    foreach (var p in session.Players)
                    {
                        _sessionHelper.BroadcastToGame(lobbyCode, cb => cb.CardDrawn(p.Nickname, p.Hand.Count));
                    }

                    _sessionHelper.BroadcastToGame(lobbyCode, cb => cb.GameMessage($"{nickname} cambió manos con {targetNickname} 🔄"));
                }
            }
        }

        private async Task<string> GetAvatarAsync(string nickname)
        {
            if (UserHelper.IsGuest(nickname))
            {
                return "LogoUNO";
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
            catch (Exception ex)
            {
                Logger.Warn($"[GAME] Error fetching avatar for {nickname}. Using default. Error: {ex.Message}");
            }

            return "LogoUNO";
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
            var session = _sessionHelper.GetGame(lobbyCode);
            var player = session.GetPlayer(nickname);
            int count = player.Hand.Count;

            _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.CardPlayed(nickname, card, count));
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
                    var selectedColor = (CardColor)context.SelectedColorId.Value;
                    session.CurrentActiveColor = selectedColor;

                    string msg = $"{context.Nickname} ha cambiado el color a {selectedColor}";
                    _sessionHelper.BroadcastToGame(session.LobbyCode, callback => callback.GameMessage(msg));
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
            int victimIndex;
            int turnIndex = 1;
            if (session.IsClockwise)
            {
                victimIndex = (session.CurrentTurnIndex + turnIndex) % session.Players.Count;
            }
            else
            {
                victimIndex = (session.CurrentTurnIndex - turnIndex + session.Players.Count) % session.Players.Count;
            }

            var victim = session.Players[victimIndex];
            var drawnCards = session.Deck.DrawCards(count);

            victim.Hand.AddRange(drawnCards);
            victim.HasSaidUno = false;

            int currentHandCount = victim.Hand.Count;
            _sessionHelper.SendToPlayer(lobbyCode, victim.Nickname, callback => callback.ReceiveCards(drawnCards));

            for (int i = 0; i < count; i++)
            {
                _sessionHelper.BroadcastToGame(lobbyCode, callback => callback.CardDrawn(victim.Nickname, currentHandCount));
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
                        await _playerRepository.UpdateMatchResultAsync(player.Nickname, isWinner, points);
                    }
                }

                _sessionHelper.BroadcastToGame(session.LobbyCode, cb => cb.MatchEnded(results));
                _sessionHelper.RemoveGame(session.LobbyCode);

                Logger.Log($"[GAME] Match {session.LobbyCode} ended. Winner: {winner.Nickname}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME] Error handling win condition for {session.LobbyCode}", ex);
            }
        }

        private List<GamePlayerData> GetRankedPlayers(GameSession session, GamePlayerData winner)
        {
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

            if (totalPlayers == 2)
            {
                if (rankIndex == 0) return 100;
                return 0;
            }
            else if (totalPlayers == 3)
            {
                if (rankIndex == 0) return 300;
                if (rankIndex == 1) return 100;
                return 0;
            }
            else if (totalPlayers == 4) 
            {
                if (rankIndex == 0) return 500;
                if (rankIndex == 1) return 300;
                if (rankIndex == 2) return 200;
                return 50;
            }

            return 0;
        }

        public Task DrawCardAsync(string lobbyCode, string nickname)
        {
            var session = _sessionHelper.GetGame(lobbyCode);
            if (session == null)
            {
                return Task.CompletedTask;
            }

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
                var playersList = session.Players.Select(p => new GamePlayer
                {
                    Nickname = p.Nickname,
                    AvatarName = p.AvatarName,
                    CardCount = p.Hand.Count
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