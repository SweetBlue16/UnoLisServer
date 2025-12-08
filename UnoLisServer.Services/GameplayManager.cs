using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Services.ManagerInterfaces;
using UnoLisServer.Services;

namespace UnoLisServer.Services
{
    /// <summary>
    /// Facade for logic manager for game
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = 
        ConcurrencyMode.Reentrant)]

    public class GameplayManager : IGameplayManager
    {
        private readonly IGameManager _gameManager;

        public GameplayManager() : this(new GameManager())
        {
        }

        public GameplayManager(IGameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void PlayCard(PlayCardData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.LobbyCode))
            {
                Logger.Warn("[GAMEPLAY] Received invalid PlayCard data (null or empty lobby).");
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    var context = new PlayCardContext(
                        data.LobbyCode,
                        data.Nickname,
                        data.CardId,
                        data.SelectedColorId
                    );

                    await _gameManager.PlayCardAsync(context);
                }
                catch (InvalidOperationException invOpEx)
                {
                    Logger.Warn($"[GAMEPLAY] Illegal move attempted: {invOpEx.Message}");
                }
                catch (ArgumentException argEx)
                {
                    Logger.Warn($"[GAMEPLAY] Invalid data processing PlayCard: {argEx.Message}");
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Error($"[GAMEPLAY] Timeout processing turn in {data.LobbyCode}", timeEx);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error during PlayCard", ex);
                }
            });
        }

        public void DrawCard(string lobbyCode, string nickname)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(nickname))
            {
                Logger.Warn("[GAMEPLAY] DrawCard called with invalid parameters.");
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    await _gameManager.DrawCardAsync(lobbyCode, nickname);
                }
                catch (InvalidOperationException invOpEx)
                {
                    Logger.Warn($"[GAMEPLAY] Illegal draw attempted: {invOpEx.Message}");
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Error($"[GAMEPLAY] Timeout processing draw", timeEx);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error during DrawCard", ex);
                }
            });
        }

        public void ConnectToGame(string lobbyCode, string nickname)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(nickname))
            {
                Logger.Warn("[GAMEPLAY] ConnectToGame failed. Invalid parameters.");
                return;
            }

            try
            {
                _gameManager.ConnectPlayer(lobbyCode, nickname);


            }
            catch (ArgumentException argEx)
            {
                Logger.Warn($"[GAMEPLAY] Connection rejected : {argEx.Message}");
            }
            catch (InvalidOperationException invEx)
            {
                Logger.Warn($"[GAMEPLAY] Connection invalid state: {invEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Error($"[GAMEPLAY] Timeout connecting player", timeEx);
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error connecting to game", ex);
            }
        }

        public void SayUnoAsync(string lobbyCode, string nickname)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    await _gameManager.SayUnoAsync(lobbyCode, nickname);
                }
                catch (InvalidOperationException ruleEx)
                {
                    Logger.Warn($"[GAMEPLAY] Say UNO failed (Rule Violation): {ruleEx.Message}");
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Warn($"[GAMEPLAY] Timeout processing Say UNO: {timeEx.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error processing UNO shout", ex);
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
                _gameManager.DisconnectPlayer(lobbyCode, nickname);
                Logger.Log($"[GAMEPLAY] Player disconnected from game {lobbyCode}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[GAMEPLAY] Timeout processing disconnection: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"[GAMEPLAY] Warning during player disconnection cleanup: {ex.Message}");
            }
        }

        public void UseItem(string lobbyCode, string nickname, ItemType itemType, string targetNickname)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(nickname))
            {
                Logger.Warn("[GAMEPLAY] UseItem failed. Missing parameters.");
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    _gameManager.UseItem(lobbyCode, nickname, itemType, targetNickname);

                    Logger.Log($"[GAMEPLAY] {nickname} used item {itemType}");
                }
                catch (InvalidOperationException ruleEx)
                {
                    Logger.Warn($"[GAMEPLAY] Item usage denied: {ruleEx.Message}");
                }
                catch (ArgumentException argEx)
                {
                    Logger.Warn($"[GAMEPLAY] Item usage failed (Invalid Target): {argEx.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error using item {itemType}", ex);
                }
            });
        }
    }
}
