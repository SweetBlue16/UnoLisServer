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

        [Fact]
        public async Task UpdatePlayerProfileAsync_BasicInfo_UpdatesDatabase()
        {
            // ARRANGE
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var updateData = new Contracts.DTOs.ProfileData
            {
                Nickname = "TikiTest",
                FullName = "Tiki Updated Name",
                Email = "updated@test.com",
                Password = null
            };

            // ACT
            await repository.UpdatePlayerProfileAsync(updateData);

            // ASSERT
            using (var context = new UNOContext(_entityConnectionString))
            {
                var updatedPlayer = context.Player.Include("Account").FirstOrDefault(p => p.nickname == "TikiTest");
                Assert.Equal("Tiki Updated Name", updatedPlayer.fullName);
                Assert.Equal("updated@test.com", updatedPlayer.Account.First().email);
            }
        }

        [Fact]
        public async Task UpdatePlayerProfileAsync_NewPassword_UpdatesHash()
        {
            // ARRANGE
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));

            string oldHash;
            using (var ctx = new UNOContext(_entityConnectionString))
                oldHash = ctx.Account.First(a => a.Player.nickname == "TikiTest").password;

            var updateData = new Contracts.DTOs.ProfileData
            {
                Nickname = "TikiTest",
                FullName = "Tiki Tester",
                Email = "tiki@test.com",
                Password = "NewStrongPassword1!"
            };

            // ACT
            await repository.UpdatePlayerProfileAsync(updateData);

            // ASSERT
            using (var context = new UNOContext(_entityConnectionString))
            {
                var updatedAccount = context.Account.First(a => a.Player.nickname == "TikiTest");
                Assert.NotEqual(oldHash, updatedAccount.password);
                // Opcional: Verificar con PasswordHelper si coincide
            }
        }

        [Fact]
        public async Task UpdatePlayerProfileAsync_UpdateExistingSocialNetwork_ChangesLink()
        {
            // ARRANGE
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var updateData = new Contracts.DTOs.ProfileData
            {
                Nickname = "TikiTest",
                FullName = "Tiki",
                Email = "tiki@test.com",
                FacebookUrl = "facebook.com/newlink"
            };

            // ACT
            await repository.UpdatePlayerProfileAsync(updateData);

            // ASSERT
            using (var context = new UNOContext(_entityConnectionString))
            {
                var sn = context.SocialNetwork.FirstOrDefault(s => s.Player.nickname == "TikiTest" && s.tipoRedSocial == "Facebook");
                Assert.Equal("facebook.com/newlink", sn.linkRedSocial);
            }
        }

        [Fact]
        public async Task UpdatePlayerProfileAsync_AddNewSocialNetwork_InsertsRecord()
        {
            // ARRANGE
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var updateData = new Contracts.DTOs.ProfileData
            {
                Nickname = "TikiTest",
                FullName = "Tiki",
                Email = "tiki@test.com",
                InstagramUrl = "instagram.com/tiki_insta"
            };

            // ACT
            await repository.UpdatePlayerProfileAsync(updateData);

            // ASSERT
            using (var context = new UNOContext(_entityConnectionString))
            {
                var sn = context.SocialNetwork.FirstOrDefault(s => s.Player.nickname == "TikiTest" && s.tipoRedSocial == "Instagram");
                Assert.NotNull(sn);
                Assert.Equal("instagram.com/tiki_insta", sn.linkRedSocial);
            }
        }

        [Fact]
        public async Task UpdatePlayerProfileAsync_EmptySocialUrl_DoesNotDeleteOrError()
        {
            // ARRANGE
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var updateData = new Contracts.DTOs.ProfileData
            {
                Nickname = "TikiTest",
                FullName = "Tiki",
                Email = "tiki@test.com",
                FacebookUrl = ""
            };

            // ACT
            await repository.UpdatePlayerProfileAsync(updateData);

            // ASSERT
            using (var context = new UNOContext(_entityConnectionString))
            {
                var sn = context.SocialNetwork.FirstOrDefault(s => s.Player.nickname == "TikiTest" && s.tipoRedSocial == "Facebook");
                Assert.NotNull(sn);
                Assert.Equal("fb.com/tiki", sn.linkRedSocial);
            }
        }

        [Fact]
        public async Task UpdatePlayerProfileAsync_NonExistentUser_ThrowsException()
        {
            // ARRANGE
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var updateData = new Contracts.DTOs.ProfileData { Nickname = "GhostUser" };

            // ACT & ASSERT
            await Assert.ThrowsAsync<Exception>(() => repository.UpdatePlayerProfileAsync(updateData));
        }

        [Fact]
        public async Task UpdatePlayerProfileAsync_DuplicateEmail_ThrowsDbUpdateException()
        {
            // ARRANGE
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));

            var updateData = new Contracts.DTOs.ProfileData
            {
                Nickname = "TikiTest",
                FullName = "Tiki",
                Email = "newbie@test.com"
            };

            // ACT & ASSERT
            await Assert.ThrowsAsync<System.Data.Entity.Infrastructure.DbUpdateException>(
                () => repository.UpdatePlayerProfileAsync(updateData)
            );
        }
    }
}