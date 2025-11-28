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
        private static readonly Lazy<GameSessionHelper> _instance =
            new Lazy<GameSessionHelper>(() => new GameSessionHelper());

        public static GameSessionHelper Instance => _instance.Value;

        private readonly Dictionary<string, GameSession> _activeGames = new Dictionary<string, GameSession>();

        private readonly Dictionary<string, List<IGameplayCallback>> _gameCallbacks = new Dictionary<string, 
            List<IGameplayCallback>>();

        private readonly object _lock = new object();

        private GameSessionHelper() { }

        public void CreateGame(string lobbyCode, GameSession session)
        {
            lock (_lock)
            {
                if (!_activeGames.ContainsKey(lobbyCode))
                {
                    _activeGames.Add(lobbyCode, session);
                    _gameCallbacks.Add(lobbyCode, new List<IGameplayCallback>());
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


        public void RegisterCallback(string lobbyCode, IGameplayCallback callback)
        {
            lock (_lock)
            {
                if (_gameCallbacks.ContainsKey(lobbyCode) && !_gameCallbacks[lobbyCode].Contains(callback))
                {
                    _gameCallbacks[lobbyCode].Add(callback);
                }
            }
        }

        public void UnregisterCallback(string lobbyCode, IGameplayCallback callback)
        {
            lock (_lock)
            {
                if (_gameCallbacks.ContainsKey(lobbyCode))
                {
                    _gameCallbacks[lobbyCode].Remove(callback);
                }
            }
        }

        public void BroadcastToGame(string lobbyCode, Action<IGameplayCallback> action)
        {
            List<IGameplayCallback> targets = null;
            var deadCallbacks = new List<IGameplayCallback>();

            lock (_lock)
            {
                if (_gameCallbacks.ContainsKey(lobbyCode))
                {
                    targets = new List<IGameplayCallback>(_gameCallbacks[lobbyCode]);
                }
            }

            if (targets == null) return;

            foreach (var cb in targets)
            {
                try
                {
                    if (cb is ICommunicationObject commObj && commObj.State == CommunicationState.Opened)
                    {
                        action(cb);
                    }
                    else
                    {
                        deadCallbacks.Add(cb);
                    }
                }
                catch (ObjectDisposedException)
                {
                    deadCallbacks.Add(cb);
                    Logger.Warn($"[GAME-BROADCAST] Client object disposed in {lobbyCode}. Marking for removal.");
                }
                catch (TimeoutException timeoutEx)
                {
                    deadCallbacks.Add(cb);
                    Logger.Warn($"[GAME-BROADCAST] Timeout broadcasting to client in {lobbyCode}: {timeoutEx.Message}");
                }
                catch (CommunicationException commEx)
                {
                    deadCallbacks.Add(cb);
                    Logger.Warn($"[GAME-BROADCAST] Communication error in {lobbyCode}: {commEx.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[GAME-BROADCAST] Critical error executing action in {lobbyCode}", ex);
                }
            }

            RemoveDeadCallbacks(lobbyCode, deadCallbacks);
        }

        private void RemoveDeadCallbacks(string lobbyCode, List<IGameplayCallback> deadCallbacks)
        {
            if (deadCallbacks == null || !deadCallbacks.Any()) return;

            lock (_lock)
            {
                if (_gameCallbacks.TryGetValue(lobbyCode, out var currentList))
                {
                    foreach (var dead in deadCallbacks)
                    {
                        currentList.Remove(dead);
                    }
                    Logger.Log($"[GAME-CLEANUP] Automatically removed {deadCallbacks.Count} zombie connections " +
                        $"from game {lobbyCode}.");
                }
            }
        }
    }
}