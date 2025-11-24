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
        public async Task TestGetPlayerProfileUserExistsReturnsBasicInfoAndAccount()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("TikiTest");

            Assert.NotNull(result);
            Assert.Equal("TikiTest", result.nickname);
            Assert.Equal("tiki@test.com", result.Account.First().email);
        }

        [Fact]
        public async Task TestGetPlayerProfileUserExistsReturnsStatsAndSocials()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("TikiTest");

            Assert.NotNull(result);
            Assert.Equal(5, result.PlayerStatistics.First().wins);
            Assert.Equal("Facebook", result.SocialNetwork.First().tipoRedSocial);
        }

        [Fact]
        public async Task TestGetPlayerProfileUserExistsReturnsNestedAvatars()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("TikiTest");

            Assert.NotNull(result);
            Assert.Equal("Gato", result.AvatarsUnlocked.First().Avatar.avatarName);
        }

        [Fact]
        public async Task TestGetPlayerProfileUserDoesNotExistReturnsNull()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("GhostUser");
            Assert.Null(result);
        }


        [Fact]
        public async Task TestGetPlayerProfileNewUserWithNoStatsReturnsEmptyListsButNotNull()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("Newbie");

            Assert.NotNull(result);
            Assert.Equal("Newbie", result.nickname);

            Assert.True(result.PlayerStatistics == null || result.PlayerStatistics.Count == 0);
            Assert.True(result.SocialNetwork == null || result.SocialNetwork.Count == 0);
        }

        [Fact]
        public async Task TestGetPlayerProfileCaseInsensitiveShouldFindUserUpperCase()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("TIKITEST");

            Assert.NotNull(result);
            Assert.Equal("TikiTest", result.nickname);
        }

        [Fact]
        public async Task TestGetPlayerProfileCaseInsensitiveShouldFindUserLowerCase()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("tikitest");

            Assert.NotNull(result);
            Assert.Equal("TikiTest", result.nickname);
        }

        [Fact]
        public async Task TestGetPlayerProfileWithLeadingSpacesShouldNotMatchExact()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));

            var result = await repository.GetPlayerProfileByNicknameAsync(" TikiTest");

            Assert.Null(result);
        }

        [Fact]
        public async Task TestGetPlayerProfileEmptyStringReturnsNull()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync("");
            Assert.Null(result);
        }

        [Fact]
        public async Task TestGetPlayerProfileNullStringReturnsNull()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var result = await repository.GetPlayerProfileByNicknameAsync(null);
            Assert.Null(result);
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncBasicInfoUpdatesDatabase()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var updateData = new Contracts.DTOs.ProfileData
            {
                Nickname = "TikiTest",
                FullName = "Tiki Updated Name",
                Email = "updated@test.com",
                Password = null
            };

            await repository.UpdatePlayerProfileAsync(updateData);

            using (var context = new UNOContext(_entityConnectionString))
            {
                var updatedPlayer = context.Player.Include("Account").FirstOrDefault(p => p.nickname == "TikiTest");
                Assert.Equal("Tiki Updated Name", updatedPlayer.fullName);
                Assert.Equal("updated@test.com", updatedPlayer.Account.First().email);
            }
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncNewPasswordUpdatesHash()
        {
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

            await repository.UpdatePlayerProfileAsync(updateData);

            using (var context = new UNOContext(_entityConnectionString))
            {
                var updatedAccount = context.Account.First(a => a.Player.nickname == "TikiTest");
                Assert.NotEqual(oldHash, updatedAccount.password);
            }
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncUpdateExistingSocialNetworkChangesLink()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var updateData = new Contracts.DTOs.ProfileData
            {
                Nickname = "TikiTest",
                FullName = "Tiki",
                Email = "tiki@test.com",
                FacebookUrl = "facebook.com/newlink"
            };

            await repository.UpdatePlayerProfileAsync(updateData);

            using (var context = new UNOContext(_entityConnectionString))
            {
                var sn = context.SocialNetwork.FirstOrDefault(s => s.Player.nickname == "TikiTest" && s.tipoRedSocial == "Facebook");
                Assert.Equal("facebook.com/newlink", sn.linkRedSocial);
            }
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncAddNewSocialNetworkInsertsRecord()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var updateData = new Contracts.DTOs.ProfileData
            {
                Nickname = "TikiTest",
                FullName = "Tiki",
                Email = "tiki@test.com",
                InstagramUrl = "instagram.com/tiki_insta"
            };

            await repository.UpdatePlayerProfileAsync(updateData);

            using (var context = new UNOContext(_entityConnectionString))
            {
                var sn = context.SocialNetwork.FirstOrDefault(s => s.Player.nickname == "TikiTest" && s.tipoRedSocial == "Instagram");
                Assert.NotNull(sn);
                Assert.Equal("instagram.com/tiki_insta", sn.linkRedSocial);
            }
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncEmptySocialUrlDoesNotDeleteOrError()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var updateData = new Contracts.DTOs.ProfileData
            {
                Nickname = "TikiTest",
                FullName = "Tiki",
                Email = "tiki@test.com",
                FacebookUrl = ""
            };

            await repository.UpdatePlayerProfileAsync(updateData);

            using (var context = new UNOContext(_entityConnectionString))
            {
                var sn = context.SocialNetwork.FirstOrDefault(s => s.Player.nickname == "TikiTest" && s.tipoRedSocial == "Facebook");
                Assert.NotNull(sn);
                Assert.Equal("fb.com/tiki", sn.linkRedSocial);
            }
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncNonExistentUserThrowsException()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var updateData = new Contracts.DTOs.ProfileData { Nickname = "GhostUser" };

            await Assert.ThrowsAsync<Exception>(() => repository.UpdatePlayerProfileAsync(updateData));
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncDuplicateEmailThrowsDbUpdateException()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));

            var updateData = new Contracts.DTOs.ProfileData
            {
                Nickname = "TikiTest",
                FullName = "Tiki",
                Email = "newbie@test.com"
            };

            await Assert.ThrowsAsync<System.Data.Entity.Infrastructure.DbUpdateException>(
                () => repository.UpdatePlayerProfileAsync(updateData)
            );
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncFullNameTooLongThrowsWrappedValidationException()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            string longName = new string('A', 300);

            var updateData = new Contracts.DTOs.ProfileData
            {
                Nickname = "TikiTest",
                FullName = longName,
                Email = "tiki@test.com"
            };

            var exception = await Assert.ThrowsAsync<Exception>(
                () => repository.UpdatePlayerProfileAsync(updateData)
            );

            Assert.Contains("Error de validación en base de datos", exception.Message);
            Assert.IsType<System.Data.Entity.Validation.DbEntityValidationException>(exception.InnerException);
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncSqlConstraintViolationRollsBackAllChanges()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));

            var updateData = new Contracts.DTOs.ProfileData
            {
                Nickname = "TikiTest",
                FullName = "NAME SHOULD NOT CHANGE",
                Email = "newbie@test.com"
            };

            await Assert.ThrowsAsync<System.Data.Entity.Infrastructure.DbUpdateException>(
                () => repository.UpdatePlayerProfileAsync(updateData)
            );

            using (var context = new UNOContext(_entityConnectionString))
            {
                var tiki = context.Player.FirstOrDefault(p => p.nickname == "TikiTest");

                if (tiki != null)
                {
                    Assert.Equal("Tiki Tester", tiki.fullName);
                }
                else
                {
                    Assert.Null(tiki);
                }
            }
        }

        [Fact]
        public async Task TestCreatePlayerAsyncValidDataInsertsPlayerAndAccount()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var newPlayer = new Contracts.DTOs.RegistrationData
            {
                Nickname = "FreshPlayer",
                FullName = "Fresh Name",
                Email = "fresh@test.com",
                Password = "StrongPassword1!"
            };

            await repository.CreatePlayerAsync(newPlayer);

            using (var context = new UNOContext(_entityConnectionString))
            {
                var player = context.Player.Include("Account").Include("PlayerStatistics").FirstOrDefault(p => p.nickname == "FreshPlayer");

                Assert.NotNull(player);
                Assert.Equal("Fresh Name", player.fullName);
                Assert.Equal(0, player.revoCoins);

                Assert.NotNull(player.Account.FirstOrDefault());
                Assert.Equal("fresh@test.com", player.Account.First().email);

                Assert.NotNull(player.PlayerStatistics.FirstOrDefault());
                Assert.Equal(0, player.PlayerStatistics.First().wins);
            }
        }

        [Fact]
        public async Task TestCreatePlayerAsyncDuplicateNicknameThrowsDbUpdateException()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var duplicatePlayer = new Contracts.DTOs.RegistrationData
            {
                Nickname = "TikiTest",
                FullName = "Imposter",
                Email = "unique@test.com",
                Password = "Pass"
            };

            await Assert.ThrowsAsync<System.Data.Entity.Infrastructure.DbUpdateException>(
                () => repository.CreatePlayerAsync(duplicatePlayer)
            );
        }

        [Fact]
        public async Task TestCreatePlayerAsyncDuplicateEmailThrowsDbUpdateException()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            var duplicateEmail = new Contracts.DTOs.RegistrationData
            {
                Nickname = "UniqueNick",
                FullName = "Imposter",
                Email = "tiki@test.com", // Duplicado
                Password = "Pass"
            };

            // ACT & ASSERT
            await Assert.ThrowsAsync<System.Data.Entity.Infrastructure.DbUpdateException>(
                () => repository.CreatePlayerAsync(duplicateEmail)
            );
        }

        [Fact]
        public async Task TestIsNicknameTakenAsyncExistingNickReturnsTrue()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            bool result = await repository.IsNicknameTakenAsync("TikiTest");
            Assert.True(result);
        }

        [Fact]
        public async Task TestIsNicknameTakenAsyncNewNickReturnsFalse()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            bool result = await repository.IsNicknameTakenAsync("FreeNick");
            Assert.False(result);
        }

        [Fact]
        public async Task TestIsEmailRegisteredAsyncExistingEmailReturnsTrue()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            bool result = await repository.IsEmailRegisteredAsync("tiki@test.com");
            Assert.True(result);
        }

        [Fact]
        public async Task TestIsEmailRegisteredAsyncNewEmailReturnsFalse()
        {
            var repository = new PlayerRepository(() => new UNOContext(_entityConnectionString));
            bool result = await repository.IsEmailRegisteredAsync("free@test.com");
            Assert.False(result);
        }
    }
}