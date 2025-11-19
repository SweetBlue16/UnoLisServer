using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Contracts.Models;
using UnoLisServer.Services.ManagerInterfaces;
using UnoLisServer.Data;

namespace UnoLisServer.Services
{
    /// <summary>
    /// Singleton implementation of ILobbyManager.
    /// Maintains a thread-safe, in-memory registry of all active lobbies.
    /// </summary>
    public class LobbyManager : ILobbyManager
    {
        private static readonly Lazy<LobbyManager> _instance =
            new Lazy<LobbyManager>(() => new LobbyManager());

        public static LobbyManager Instance => _instance.Value;

        private readonly Dictionary<string, LobbyInfo> _activeLobbies = new Dictionary<string, LobbyInfo>();

        private readonly Dictionary<string,List<ILobbyDuplexCallback>> _lobbyCallbacks =
            new Dictionary<string, List<ILobbyDuplexCallback>>();

        private readonly object _dictionaryLock = new object();
        private readonly Random _random = new Random();

        private LobbyManager() 
        { 
        }

        public CreateMatchResponse CreateLobby(MatchSettings settings)
        {
            lock (_dictionaryLock)
            {
                try
                {
                    string newCode = GenerateUniqueLobbyCode();
                    var newLobby = new LobbyInfo(newCode, settings);

                    string hostAvatar = GetPlayerAvatarFromDb(settings.HostNickname);
                    newLobby.AddPlayer(settings.HostNickname, hostAvatar);

                    _activeLobbies.Add(newCode, newLobby);
                    _lobbyCallbacks.Add(newCode, new List<ILobbyDuplexCallback>());

                    Logger.Log($"Lobby Created. Code: {newCode}. Host: {settings.HostNickname}.");

                    return new CreateMatchResponse
                    {
                        Success = true,
                        LobbyCode = newCode,
                        Message = "Lobby created successfully."
                    };
                }
                catch (Exception ex)
                {
                    Logger.Error("Error creating lobby.", ex);
                    return new CreateMatchResponse
                    {
                        Success = false,
                        Message = "Failed to create lobby."
                    };
                }
            }
        }

        public JoinMatchResponse JoinLobby(string lobbyCode, string nickname)
        {
            LobbyInfo lobby;

            lock (_dictionaryLock)
            {
                if (!_activeLobbies.TryGetValue(lobbyCode, out lobby))
                {
                    return new JoinMatchResponse
                    {
                        Success = false,
                        Message = "Lobby not found.",
                        LobbyCode = null
                    };
                }
            }

            string avatarName = GetPlayerAvatarFromDb(nickname);
            bool joined = lobby.AddPlayer(nickname, avatarName);

            if (joined)
            {
                Logger.Log($"Player {nickname} joined lobby {lobbyCode}.");
                return new JoinMatchResponse
                {
                    Success = true,
                    LobbyCode = lobbyCode,
                    Message = "Joined successfully."
                };
            }

            return new JoinMatchResponse
            {
                Success = false,
                Message = "Lobby is full or player already exists.",
                LobbyCode = null
            };
        }

        public void RegisterPlayerConnection(string lobbyCode, string nickname)
        {
            var callback = OperationContext.Current.GetCallbackChannel<ILobbyDuplexCallback>();

            string currentAvatar = SafeRegisterCallback(lobbyCode, nickname, callback);
            SendInitialStateToPlayer(lobbyCode, callback);

            BroadcastToLobby(lobbyCode, cb => cb.PlayerJoined(nickname, currentAvatar));

            if (_activeLobbies.TryGetValue(lobbyCode, out var lobbyInfo))
            {
                var list = lobbyInfo.Players.ToArray();
                BroadcastToLobby(lobbyCode, cb => cb.UpdatePlayerList(list));
            }
        }

        public void RemovePlayerConnection(string lobbyCode, string nickname)
        {
            var callback = OperationContext.Current.GetCallbackChannel<ILobbyDuplexCallback>();

            lock (_dictionaryLock)
            {
                if (_lobbyCallbacks.ContainsKey(lobbyCode))
                {
                    _lobbyCallbacks[lobbyCode].Remove(callback);
                }

                if (_activeLobbies.TryGetValue(lobbyCode, out var lobby))
                {
                    lobby.RemovePlayer(nickname);
                }
            }

            BroadcastToLobby(lobbyCode, cb => cb.PlayerLeft(nickname));

            if (_activeLobbies.TryGetValue(lobbyCode, out var lobbyInfo))
            {
                var namesList = lobbyInfo.Players.ToArray();
                BroadcastToLobby(lobbyCode, cb => cb.UpdatePlayerList(namesList));
            }
        }

        private string GetPlayerAvatarFromDb(string nickname)
        {
            try
            {
                using (var context = new UNOContext())
                {
                    var player = context.Player.Include("AvatarsUnlocked.Avatar").FirstOrDefault(p => p.nickname == 
                    nickname);

                    if (player != null && player.SelectedAvatar_Avatar_idAvatar != null)
                    {
                        var unlocked = player.AvatarsUnlocked
                            .FirstOrDefault(au => au.Avatar_idAvatar == player.SelectedAvatar_Avatar_idAvatar);

                        if (unlocked != null && unlocked.Avatar != null)
                        {
                            return unlocked.Avatar.avatarName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error fetching avatar for {nickname}: {ex.Message}", ex);
            }

            return "LogoUNO";
        }

        private string SafeRegisterCallback(string lobbyCode, string nickname, ILobbyDuplexCallback callback)
        {
            string avatar = "LogoUNO";

            lock (_dictionaryLock)
            {
                if (!_activeLobbies.ContainsKey(lobbyCode)) return avatar;

                if (!_lobbyCallbacks.ContainsKey(lobbyCode))
                    _lobbyCallbacks[lobbyCode] = new List<ILobbyDuplexCallback>();

                if (!_lobbyCallbacks[lobbyCode].Contains(callback))
                {
                    _lobbyCallbacks[lobbyCode].Add(callback);
                    Logger.Log($"Player {nickname} connected duplex to {lobbyCode}");
                }

                if (_activeLobbies.TryGetValue(lobbyCode, out var lobby))
                {
                    var p = lobby.Players.FirstOrDefault(x => x.Nickname == nickname);
                    if (p != null) avatar = p.AvatarName;
                }
            }
            return avatar;
        }

        private void SendInitialStateToPlayer(string lobbyCode, ILobbyDuplexCallback callback)
        {
            if (_activeLobbies.TryGetValue(lobbyCode, out var lobbyInfo))
            {
                try
                {
                    callback.UpdatePlayerList(lobbyInfo.Players.ToArray());
                }
                catch
                {
                    Logger.Error("Error sending initial player list to newly connected player.");
                }
            }
        }
        public void BroadcastReadyStatus(string lobbyCode, string nickname, bool isReady)
        {
            bool allReady = false;
            int playerCount = 0;
            lock(_dictionaryLock)
            {
                if (_activeLobbies.TryGetValue(lobbyCode, out var lobby))
                {
                    var player = lobby.Players.FirstOrDefault(p => p.Nickname == nickname);
                    if (player != null)
                    {
                        player.IsReady = isReady;
                    }

                    playerCount = lobby.Players.Count;

                    if (playerCount >= 2 && lobby.Players.All(p => p.IsReady))
                    {
                        allReady = true;
                    }
                }
            }

            BroadcastToLobby(lobbyCode, cb => cb.PlayerReadyStatusChanged(nickname, isReady));

            if (allReady)
            {
                Logger.Log($"All players ready in lobby {lobbyCode}. Starting game.");
                System.Threading.Thread.Sleep(5000);
                BroadcastToLobby(lobbyCode, cb => cb.GameStarted());
            }
        }

        private void BroadcastToLobby(string lobbyCode, Action<ILobbyDuplexCallback> action)
        {
            List<ILobbyDuplexCallback> targets = null;

            lock (_dictionaryLock)
            {
                if (_lobbyCallbacks.ContainsKey(lobbyCode))
                {
                    targets = new List<ILobbyDuplexCallback>(_lobbyCallbacks[lobbyCode]);
                }
            }

            if (targets == null) return;

            foreach (var cb in targets)
            {
                try
                {
                    if (((ICommunicationObject)cb).State == CommunicationState.Opened)
                    {
                        action(cb);
                    }
                }
                catch 
                {
                    Logger.Error("Error broadcasting to lobby callbacks.");
                }
            }
        }
        private string GenerateUniqueLobbyCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string code;
            do
            {
                code = new string(Enumerable.Repeat(chars, 5)
                  .Select(s => s[_random.Next(s.Length)]).ToArray());
            } 
            while (_activeLobbies.ContainsKey(code));

            return code;
        }
    }
}
