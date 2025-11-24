using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using Xunit;

namespace UnoLisServer.Test
{
    public class PlayerRepositoryTest : IDisposable
    {
        private readonly TransactionScope _scope;
        private readonly string _entityConnectionString;

        public PlayerRepositoryTest()
        {
            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = ".",
                InitialCatalog = "UNOLIS_TEST",
                IntegratedSecurity = true,
                MultipleActiveResultSets = true,
                ApplicationName = "EntityFramework"
            };

            string assemblyName = typeof(PlayerRepository).Assembly.GetName().Name;

            var entityBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = sqlBuilder.ToString(),
                Metadata = $"res://*/UNODataBaseModel.csdl|res://*/UNODataBaseModel.ssdl|res://*/UNODataBaseModel.msl"
            };

            _entityConnectionString = entityBuilder.ToString();

            _scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            using (var context = new UNOContext(_entityConnectionString))
            {
                var playerFull = new Player
                {
                    nickname = "TikiTest",
                    fullName = "Tiki Tester",
                    revoCoins = 100,
                    Account = new List<Account>(),
                    PlayerStatistics = new List<PlayerStatistics>(),
                    SocialNetwork = new List<SocialNetwork>(),
                    AvatarsUnlocked = new List<AvatarsUnlocked>()
                };

                playerFull.Account.Add(new Account { email = "tiki@test.com", password = "hashed" });
                playerFull.PlayerStatistics.Add(new PlayerStatistics { wins = 5, matchesPlayed = 10 });
                playerFull.SocialNetwork.Add(new SocialNetwork { tipoRedSocial = "Facebook", linkRedSocial = "fb.com/tiki" });

                var avatar = new Avatar { avatarName = "Gato", avatarRarity = "Common" };
                playerFull.AvatarsUnlocked.Add(new AvatarsUnlocked { Avatar = avatar, unlockedDate = DateTime.Now });

                var playerEmpty = new Player
                {
                    nickname = "Newbie",
                    fullName = "Noob Tester",
                    revoCoins = 0,
                    Account = new List<Account>(),
                    PlayerStatistics = new List<PlayerStatistics>(),
                    SocialNetwork = new List<SocialNetwork>(),
                    AvatarsUnlocked = new List<AvatarsUnlocked>()
                };
                playerEmpty.Account.Add(new Account { email = "newbie@test.com", password = "hashed" });

                context.Player.Add(playerFull);
                context.Player.Add(playerEmpty);
                context.Avatar.Add(avatar); 

                context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _scope.Dispose();
        }

        [Fact]
        public async Task GetPlayerProfile_UserExists_ReturnsBasicInfoAndAccount()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("TikiTest");

            Assert.NotNull(result);
            Assert.Equal("TikiTest", result.nickname);
            Assert.Equal("tiki@test.com", result.Account.First().email);
        }

        [Fact]
        public async Task GetPlayerProfile_UserExists_ReturnsStatsAndSocials()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("TikiTest");

            Assert.NotNull(result);
            Assert.Equal(5, result.PlayerStatistics.First().wins);
            Assert.Equal("Facebook", result.SocialNetwork.First().tipoRedSocial);
        }

        [Fact]
        public async Task GetPlayerProfile_UserExists_ReturnsNestedAvatars()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("TikiTest");

            Assert.NotNull(result);
            Assert.Equal("Gato", result.AvatarsUnlocked.First().Avatar.avatarName);
        }

        [Fact]
        public async Task GetPlayerProfile_UserDoesNotExist_ReturnsNull()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("GhostUser");
            Assert.Null(result);
        }


        [Fact]
        public async Task GetPlayerProfile_NewUserWithNoStats_ReturnsEmptyListsButNotNull()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("Newbie");

            Assert.NotNull(result);
            Assert.Equal("Newbie", result.nickname);

            Assert.True(result.PlayerStatistics == null || result.PlayerStatistics.Count == 0);
            Assert.True(result.SocialNetwork == null || result.SocialNetwork.Count == 0);
        }

        [Fact]
        public async Task GetPlayerProfile_CaseInsensitive_ShouldFindUserUpperCase()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("TIKITEST");

            Assert.NotNull(result);
            Assert.Equal("TikiTest", result.nickname);
        }

        [Fact]
        public async Task GetPlayerProfile_CaseInsensitive_ShouldFindUserLowerCase()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("tikitest");

            Assert.NotNull(result);
            Assert.Equal("TikiTest", result.nickname);
        }

        [Fact]
        public async Task GetPlayerProfile_WithLeadingSpaces_ShouldNotMatchExact()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));

            var result = await repository.GetPlayerProfileByNicknameAsync(" TikiTest");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetPlayerProfile_EmptyString_ReturnsNull()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPlayerProfile_NullString_ReturnsNull()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync(null);
            Assert.Null(result);
        }
    }
}