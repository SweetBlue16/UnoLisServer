using System;
using System.Collections.Generic;

namespace UnoLisServer.Data.Factories 
{
    public interface IPlayerFactory
    {
        Player CreateNewPlayer(string nickname, string fullName, string email, string passwordHash);
    }

    public class PlayerFactory : IPlayerFactory
    {
        private readonly int[] _defaultAvatarIds = { 1, 10, 11 };
        private const int InitialCoins = 0;

        public Player CreateNewPlayer(string nickname, string fullName, string email, string passwordHash)
        {
            var player = new Player
            {
                nickname = nickname,
                fullName = fullName,
                revoCoins = InitialCoins,
                AvatarsUnlocked = new List<AvatarsUnlocked>(),
                Account = new List<Account>(),
                PlayerStatistics = new List<PlayerStatistics>()
            };

            var account = new Account
            {
                email = email,
                password = passwordHash,
                Player = player
            };
            player.Account.Add(account);

            var stats = new PlayerStatistics
            {
                Player = player,
                wins = 0,
                matchesPlayed = 0,
                globalPoints = 0,
                loses = 0,
                streak = 0,
                maxStreak = 0
            };
            player.PlayerStatistics.Add(stats);

            var now = DateTime.UtcNow;
            foreach (var avatarId in _defaultAvatarIds)
            {
                player.AvatarsUnlocked.Add(new AvatarsUnlocked
                {
                    Player = player,
                    Avatar_idAvatar = avatarId,
                    unlockedDate = now
                });
            }

            return player;
        }
    }
}