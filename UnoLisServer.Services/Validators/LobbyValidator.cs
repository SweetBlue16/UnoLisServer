using System;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Services.Validators
{
    public static class LobbyValidator
    {
        private const int MaxPlayerCount = 4;
        private const int MinPlayerCount = 2;

        public static void ValidateSettings(MatchSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("Settings cannot be null");
            }

            if (string.IsNullOrWhiteSpace(settings.HostNickname))
            {
                throw new ArgumentException("Host nickname is required");
            }

            if (settings.MaxPlayers < MinPlayerCount || settings.MaxPlayers > MaxPlayerCount) 
            {
                throw new ArgumentException("Max players must be between 2 and 4");
            }
        }

        public static void ValidateJoinRequest(string code, string nickname)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Lobby code required");
            }

            if (string.IsNullOrWhiteSpace(nickname))
            {
                throw new ArgumentException("Nickname required");
            }
        }
    }
}