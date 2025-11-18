using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Models;
using UnoLisServer.Services.ManagerInterfaces;

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
        private readonly object _dictionaryLock = new object();
        private readonly Random _random = new Random();

        private LobbyManager() { }

        public CreateMatchResponse CreateLobby(MatchSettings settings)
        {
            lock (_dictionaryLock)
            {
                try
                {
                    string newCode = GenerateUniqueLobbyCode();
                    var newLobby = new LobbyInfo(newCode, settings);
                    _activeLobbies.Add(newCode, newLobby);

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

            bool joined = lobby.AddPlayer(nickname);

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
            else
            {
                return new JoinMatchResponse
                {
                    Success = false,
                    Message = "Lobby is full or player already exists.",
                    LobbyCode = null
                };
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
