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
        private readonly Dictionary<string, List<ILobbyDuplexCallback>> _lobbyCallbacks = new Dictionary<string,
            List<ILobbyDuplexCallback>>();

        private readonly object _lock = new object();

        private LobbySessionHelper() { }

        public void AddLobby(string code, LobbyInfo lobby)
        {
            lock (_lock)
            {
                if (!_activeLobbies.ContainsKey(code))
                {
                    _activeLobbies.Add(code, lobby);
                    _lobbyCallbacks.Add(code, new List<ILobbyDuplexCallback>());
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

        public void RegisterCallback(string code, ILobbyDuplexCallback callback)
        {
            lock (_lock)
            {
                if (_lobbyCallbacks.ContainsKey(code) && !_lobbyCallbacks[code].Contains(callback))
                {
                    _lobbyCallbacks[code].Add(callback);
                }
            }
        }

        public void UnregisterCallback(string code, ILobbyDuplexCallback callback)
        {
            lock (_lock)
            {
                if (_lobbyCallbacks.ContainsKey(code))
                {
                    _lobbyCallbacks[code].Remove(callback);
                }
            }
        }

        public void BroadcastToLobby(string code, Action<ILobbyDuplexCallback> action)
        {
            List<ILobbyDuplexCallback> targets = null;
            lock (_lock)
            {
                if (_lobbyCallbacks.ContainsKey(code))
                {
                    targets = new List<ILobbyDuplexCallback>(_lobbyCallbacks[code]);
                }
            }

            if (targets == null)
            {
                return;
            }

            foreach (var cb in targets)
            {
                try
                {
                    if (cb is ICommunicationObject commObj && commObj.State == CommunicationState.Opened)
                    {
                        action(cb);
                    }
                }
                catch (ObjectDisposedException)
                {
                    Logger.Warn($"[LOBBY-BROADCAST] Client object disposed in {code} during broadcast.");
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Warn($"[LOBBY-BROADCAST] Timeout sending to a client in {code}: {timeoutEx.Message}");
                }
                catch (CommunicationException commEx)
                {
                    Logger.Warn($"[LOBBY-BROADCAST] Communication error with client in {code}: {commEx.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[LOBBY-BROADCAST] Critical error executing broadcast action in {code}", ex);
                }
            }
        }
    }
}