using System.Linq;
using UnoLisServer.Data.Factories;
using Xunit;

namespace UnoLisServer.Test.FactoriesTest
{
    public class PlayerFactoryTest
    {
        private readonly PlayerFactory _factory;

        public PlayerFactoryTest()
        {
            _factory = new PlayerFactory();
        }

        [Fact]
        public void TestCreateNewPlayerSetsBasicPropertiesCorrectly()
        {
            var player = _factory.CreateNewPlayer("Nick", "Name", "e@mail.com", "Hash");

            Assert.Equal("Nick", player.nickname);
            Assert.Equal("Name", player.fullName);
            Assert.Equal(0, player.revoCoins);
        }

        [Fact]
        public void TestCreateNewPlayerCreatesAccountWithHash()
        {
            var player = _factory.CreateNewPlayer("Nick", "Name", "e@mail.com", "Hash123");

            Assert.NotNull(player.Account);
            Assert.Single(player.Account);
            Assert.Equal("e@mail.com", player.Account.First().email);
            Assert.Equal("Hash123", player.Account.First().password);
        }

        [Fact]
        public void TestCreateNewPlayerInitializesStatsToZero()
        {
            var player = _factory.CreateNewPlayer("Nick", "Name", "e@mail.com", "Hash");

            Assert.NotNull(player.PlayerStatistics);
            Assert.Single(player.PlayerStatistics);

            var stats = player.PlayerStatistics.First();
            Assert.Equal(0, stats.wins);
            Assert.Equal(0, stats.matchesPlayed);
            Assert.Equal(0, stats.globalPoints);
        }

        [Fact]
        public void TestCreateNewPlayerUnlocksDefaultAvatars()
        {
            var player = _factory.CreateNewPlayer("Nick", "Name", "e@mail.com", "Hash");

            Assert.NotNull(player.AvatarsUnlocked);
            Assert.Equal(3, player.AvatarsUnlocked.Count);

            Assert.Contains(player.AvatarsUnlocked, a => a.Avatar_idAvatar == 1);
            Assert.Contains(player.AvatarsUnlocked, a => a.Avatar_idAvatar == 10);
            Assert.Contains(player.AvatarsUnlocked, a => a.Avatar_idAvatar == 11);
        }
    }
}