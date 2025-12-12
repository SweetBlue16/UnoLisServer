using System.Collections.Generic;
using System.Linq;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Models
{
    /// <summary>
    /// Represents an active game lobby, living in the server's memory.
    /// This is NOT a DataContract as it is not sent directly to the client.
    /// </summary>
    public class LobbyInfo
    {
        public string LobbyCode { get; set; }
        public MatchSettings Settings { get; set; }
        public List<LobbyPlayerData> Players { get; set; }
        public string SelectedBackgroundVideo { get; set; }

        public readonly object LobbyLock = new object();

        public LobbyInfo(string lobbyCode, MatchSettings settings)
        {
            LobbyCode = lobbyCode;
            Settings = settings;
            Players = new List<LobbyPlayerData>();
            SelectedBackgroundVideo = "default_video.mp4";
        }

        public LobbyInfo() { }

        public bool AddPlayer(string nickname, string avatarName)
        {
            lock (LobbyLock)
            {
                if (Players.Count >= Settings.MaxPlayers)
                {
                    return false;
                }

                var existingPlayer = Players.FirstOrDefault(p => p.Nickname == nickname);
                if (existingPlayer != null)
                {
                    existingPlayer.AvatarName = avatarName;
                    return true;
                }

                Players.Add(new LobbyPlayerData
                {
                    Nickname = nickname,
                    AvatarName = avatarName
                });

                return true;
            }
        }

        public void RemovePlayer(string nickname)
        {
            lock (LobbyLock)
            {
                var playerToRemove = Players.FirstOrDefault(p => p.Nickname == nickname);
                if (playerToRemove != null)
                {
                    Players.Remove(playerToRemove);
                }
            }
        }
    }
}