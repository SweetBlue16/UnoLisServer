using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Contracts.Models;

namespace UnoLisServer.Services.Helpers
{
    public class LobbySessionHelper
    {
        private static readonly Lazy<LobbySessionHelper> _instance =
            new Lazy<LobbySessionHelper>(() => new LobbySessionHelper());

        public static LobbySessionHelper Instance => _instance.Value;

        private readonly Dictionary<string, LobbyInfo> _activeLobbies = new Dictionary<string, LobbyInfo>();
        private readonly Dictionary<string, Dictionary<string, ILobbyDuplexCallback>> _lobbyCallbacks =
            new Dictionary<string, Dictionary<string, ILobbyDuplexCallback>>();

        private readonly object _lock = new object();

        private LobbySessionHelper() { }

        public void AddLobby(string code, LobbyInfo lobby)
        {
            lock (_lock)
            {
                if (!_activeLobbies.ContainsKey(code))
                {
                    _activeLobbies.Add(code, lobby);
                    _lobbyCallbacks.Add(code, new Dictionary<string, ILobbyDuplexCallback>());
                }
            }
        }

        public LobbyInfo GetLobby(string code)
        {
            lock (_lock)
            {
                return _activeLobbies.TryGetValue(code, out var lobby) ? lobby : null;
            }
        }

        public bool LobbyExists(string code)
        {
            lock (_lock) return _activeLobbies.ContainsKey(code);
        }

        public void RemoveLobby(string code)
        {
            lock (_lock)
            {
                _activeLobbies.Remove(code);
                _lobbyCallbacks.Remove(code);
            }
        }

        public void RegisterCallback(string code, string nickname, ILobbyDuplexCallback callback)
        {
            lock (_lock)
            {
                if (_lobbyCallbacks.ContainsKey(code))
                {
                    _lobbyCallbacks[code][nickname] = callback;
                }
            }
        }

        public void UnregisterCallback(string code, string nickname)
        {
            lock (_lock)
            {
                if (_lobbyCallbacks.ContainsKey(code))
                {
                    if (_lobbyCallbacks[code].ContainsKey(nickname))
                    {
                        _lobbyCallbacks[code].Remove(nickname);
                    }
                }
            }
        }

        public void BroadcastToLobby(string code, Action<ILobbyDuplexCallback> action)
        {
            List<ILobbyDuplexCallback> targets = null;
            List<string> deadNicknames = new List<string>();

            lock (_lock)
            {
                if (_lobbyCallbacks.ContainsKey(code))
                {
                    targets = _lobbyCallbacks[code].Values.ToList();
                }
            }

            if (targets == null)
            {
                return;
            }

            Dictionary<string, ILobbyDuplexCallback> currentSnapshot;
            lock (_lock)
            {
                if (!_lobbyCallbacks.ContainsKey(code)) return;
                currentSnapshot = new Dictionary<string, ILobbyDuplexCallback>(_lobbyCallbacks[code]);
            }

            foreach (var kvp in currentSnapshot)
            {
                var nickname = kvp.Key;
                var callback = kvp.Value;

                try
                {
                    if (callback is ICommunicationObject commObj && commObj.State == CommunicationState.Opened)
                    {
                        action(callback);
                    }
                    else
                    {
                        deadNicknames.Add(nickname);
                    }
                }
                catch (ObjectDisposedException)
                {
                    deadNicknames.Add(nickname);
                    Logger.Warn($"[LOBBY-BROADCAST] Client object disposed in {code} during broadcast.");
                }
                catch (TimeoutException timeoutEx)
                {
                    deadNicknames.Add(nickname);
                    Logger.Warn($"[LOBBY-BROADCAST] Timeout sending to a client in {code}: {timeoutEx.Message}");
                }
                catch (CommunicationException commEx)
                {
                    deadNicknames.Add(nickname);
                    Logger.Warn($"[LOBBY-BROADCAST] Communication error with client in {code}: {commEx.Message}");
                }
                catch (Exception ex)
                {
                    deadNicknames.Add(nickname);
                    Logger.Error($"[LOBBY-BROADCAST] Critical error executing broadcast action in {code}", ex);
                }
            }

            if (deadNicknames.Any())
            {
                RemoveDeadCallbacks(code, deadNicknames);
            }
        }
        private void RemoveDeadCallbacks(string code, List<string> deadNicknames)
        {
            lock (_lock)
            {
                if (_lobbyCallbacks.ContainsKey(code))
                {
                    foreach (var nick in deadNicknames)
                    {
                        if (_lobbyCallbacks[code].ContainsKey(nick))
                        {
                            _lobbyCallbacks[code].Remove(nick);
                            Logger.Log($"[LOBBY-CLEANUP] Removed zombie session for {nick} in {code}");
                        }
                    }
                }
            }
        }
    }
}