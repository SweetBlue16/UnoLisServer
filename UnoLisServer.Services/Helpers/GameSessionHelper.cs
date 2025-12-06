using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Services.GameLogic;

namespace UnoLisServer.Services.Helpers
{
    public class GameSessionHelper
    {
        public event Action<string, string> OnPlayerDisconnected;
        private static readonly Lazy<GameSessionHelper> _instance =
            new Lazy<GameSessionHelper>(() => new GameSessionHelper());
        public static GameSessionHelper Instance => _instance.Value;

        private readonly Dictionary<string, GameSession> _activeGames = new Dictionary<string, GameSession>();
        private readonly Dictionary<string, Dictionary<string, IGameplayCallback>> _gameCallbacks =
            new Dictionary<string, Dictionary<string, IGameplayCallback>>();
        private readonly object _lock = new object();

        private GameSessionHelper() { }

        public void CreateGame(string lobbyCode, GameSession session)
        {
            lock (_lock)
            {
                if (!_activeGames.ContainsKey(lobbyCode))
                {
                    _activeGames.Add(lobbyCode, session);
                    _gameCallbacks.Add(lobbyCode, new Dictionary<string, IGameplayCallback>());
                    Logger.Log($"[GAME-SESSION] Session created for {lobbyCode}");
                }
            }
        }

        public GameSession GetGame(string lobbyCode)
        {
            lock (_lock)
            {
                return _activeGames.TryGetValue(lobbyCode, out var game) ? game : null;
            }
        }

        public void RemoveGame(string lobbyCode)
        {
            lock (_lock)
            {
                if (_activeGames.TryGetValue(lobbyCode, out var session))
                {
                    session.Dispose();
                    _activeGames.Remove(lobbyCode);
                }
                _gameCallbacks.Remove(lobbyCode);
                Logger.Log($"[GAME-SESSION] Session removed for {lobbyCode}");
            }
        }


        public void RegisterCallback(string lobbyCode, string nickname, IGameplayCallback callback)
        {
            lock (_lock)
            {
                if (!_gameCallbacks.ContainsKey(lobbyCode))
                {
                    return;
                }

                _gameCallbacks[lobbyCode][nickname] = callback;
            }
        }

        public void UnregisterCallback(string lobbyCode, string nickname)
        {
            lock (_lock)
            {
                if (_gameCallbacks.ContainsKey(lobbyCode))
                {
                    _gameCallbacks[lobbyCode].Remove(nickname);
                }
            }
        }

        public void SendToPlayer(string lobbyCode, string nickname, Action<IGameplayCallback> action)
        {
            IGameplayCallback target = null;
            lock (_lock)
            {
                if (_gameCallbacks.ContainsKey(lobbyCode) && _gameCallbacks[lobbyCode].ContainsKey(nickname))
                {
                    target = _gameCallbacks[lobbyCode][nickname];
                }
            }

            if (target != null)
            {
                ExecuteSafe(target, action, lobbyCode, nickname);
            }
        }

        public void BroadcastToGame(string lobbyCode, Action<IGameplayCallback> action)
        {
            List<KeyValuePair<string, IGameplayCallback>> targets = null;

            lock (_lock)
            {
                if (_gameCallbacks.ContainsKey(lobbyCode))
                {
                    targets = _gameCallbacks[lobbyCode].ToList();
                }
            }

            if (targets == null) return;

            foreach (var kvp in targets)
            {
                ExecuteSafe(kvp.Value, action, lobbyCode, kvp.Key);
            }
        }

        private void ExecuteSafe(IGameplayCallback callback, Action<IGameplayCallback> action, string lobbyCode, string nickname)
        {
            try
            {
                if (callback is ICommunicationObject commObj && commObj.State == CommunicationState.Opened)
                {
                    action(callback);
                }
                else
                {
                    HandleDisconnection(lobbyCode, nickname);
                }
            }
            catch (ObjectDisposedException)
            {
                Logger.Warn($"[GAME-BROADCAST] Client object disposed in {lobbyCode}. Marking for removal.");
            }
            catch (TimeoutException timeoutEx)
            {
                Logger.Warn($"[GAME-BROADCAST] Timeout broadcasting to client in {lobbyCode}: {timeoutEx.Message}");
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[GAME-BROADCAST] Communication error in {lobbyCode}: {commEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[GAME-BROADCAST] Critical error executing action in {lobbyCode}", ex);
                Logger.Warn($"[GAME-SEND] Error sending to {nickname}: {ex.Message}");
                if (ex is CommunicationException || ex is TimeoutException || ex is ObjectDisposedException)
                {
                    UnregisterCallback(lobbyCode, nickname);
                }
            }
        }

        private void HandleDisconnection(string lobbyCode, string nickname)
        {
            if (!_gameCallbacks.ContainsKey(lobbyCode) || !_gameCallbacks[lobbyCode].ContainsKey(nickname))
            {
                return;
            }

            Logger.Log($"[GAME] Detected disconnection of {nickname} in {lobbyCode}. Handling removal...");

            UnregisterCallback(lobbyCode, nickname);

            // 2. Avisar al GameManager para que actualice la lógica del juego
            // Necesitamos una forma de llamar al GameManager. 
            // Opción rápida: Evento estático o Inyección. 
            // Opción limpia: GameSessionHelper dispara un evento.
            OnPlayerDisconnected?.Invoke(lobbyCode, nickname);
        }
    }
}