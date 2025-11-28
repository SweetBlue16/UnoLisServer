using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
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

        public GameManager() : this(GameSessionHelper.Instance) { }

        public GameManager(GameSessionHelper sessionHelper)
        {
            _sessionHelper = sessionHelper;
        }

        public bool InitializeGame(string lobbyCode, List<string> playerNicknames)
        {
            try
            {
                var session = new GameSession(lobbyCode, playerNicknames);
                session.OnTurnTimeExpired += (nick) => HandleTurnExpired(lobbyCode, nick);
                session.StartGame();
                _sessionHelper.CreateGame(lobbyCode, session);

                return true;
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
                _sessionHelper.RegisterCallback(lobbyCode, callback);

                var session = _sessionHelper.GetGame(lobbyCode);
                if (session == null)
                {
                    Logger.Warn($"[GAME] Player {nickname} connected but game session {lobbyCode} not found.");
                    return;
                }

                var player = session.GetPlayer(nickname);
                if (player == null)
                {
                    Logger.Warn($"[GAME] Player {nickname} connected but is not in the player list of {lobbyCode}.");
                    return;
                }

                SendInitialStateToPlayer(callback, session, player);
                Logger.Log($"[GAME] Player {nickname} successfully connected to game {lobbyCode}");
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
            // Lógica para remover callback y pausar juego si es necesario
            // O simplemente marcar como desconectado.
        }

        public Task PlayCardAsync(string lobbyCode, string nickname, string cardId, int? selectedColorId) => 
            Task.CompletedTask;
        public Task DrawCardAsync(string lobbyCode, string nickname) => Task.CompletedTask;
        public Task SayUnoAsync(string lobbyCode, string nickname) => Task.CompletedTask;

        private void HandleTurnExpired(string lobbyCode, string nickname)
        {
            Logger.Log($"[GAME] Time expired for {nickname} in {lobbyCode}");
        }

        private void SendInitialStateToPlayer(IGameplayCallback callback, GameSession session, GamePlayerData player)
        {
            try
            {
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