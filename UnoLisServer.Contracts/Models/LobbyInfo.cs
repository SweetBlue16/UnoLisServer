using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public List<string> Players { get; set; }
        public string SelectedBackgroundVideo { get; set; }

        /// <summary>
        /// Lock object for thread-safe operations on this specific lobby instance.
        /// </summary>
        public readonly object LobbyLock = new object();

        public LobbyInfo(string lobbyCode, MatchSettings settings)
        {
            LobbyCode = lobbyCode;
            Settings = settings;
            Players = new List<string> { settings.HostNickname };
            SelectedBackgroundVideo = "default_video.mp4";
        }

        /// <summary>
        /// Attempts to add a player to this lobby.
        /// </summary>
        /// <returns>True if the player was added or already existed, false if the lobby was full.</returns>
        public bool AddPlayer(string nickname)
        {
            lock (LobbyLock)
            {
                if (Players.Count >= Settings.MaxPlayers)
                {
                    return false;
                }
                if (Players.Contains(nickname))
                {
                    return true; // Player is already here (reconnecting)
                }

                Players.Add(nickname);
                return true;
            }
        }

        /// <summary>
        /// Removes a player from this lobby.
        /// </summary>
        public void RemovePlayer(string nickname)
        {
            lock (LobbyLock)
            {
                Players.Remove(nickname);
            }
        }
    }
}