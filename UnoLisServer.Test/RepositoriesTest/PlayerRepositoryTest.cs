using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using UnoLisServer.Common.Exceptions; 
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Test.Common;
using Xunit;

namespace UnoLisServer.Test
{
    public class PlayerRepositoryTest : UnoLisTestBase
    {
        public PlayerRepositoryTest()
        {
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            using (var context = GetContext())
            {
                try
                {
                    if (context.Database.Connection.State != System.Data.ConnectionState.Open)
                        context.Database.Connection.Open();

                    context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Avatar] ON");

                    context.Database.ExecuteSqlCommand(@"
                        IF NOT EXISTS (SELECT 1 FROM [dbo].[Avatar] WHERE idAvatar = 1)
                            INSERT INTO [dbo].[Avatar] (idAvatar, avatarName, avatarRarity) VALUES (1, 'Default', 'Common');
                        
                        IF NOT EXISTS (SELECT 1 FROM [dbo].[Avatar] WHERE idAvatar = 10)
                            INSERT INTO [dbo].[Avatar] (idAvatar, avatarName, avatarRarity) VALUES (10, 'Premio1', 'Rare');

                        IF NOT EXISTS (SELECT 1 FROM [dbo].[Avatar] WHERE idAvatar = 11)
                            INSERT INTO [dbo].[Avatar] (idAvatar, avatarName, avatarRarity) VALUES (11, 'Premio2', 'Epic');
                    ");

                    context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT [dbo].[Avatar] OFF");

                    var defaultAvatar = context.Avatar.Find(1);

                    var playerFull = BuildFullPlayer(defaultAvatar);
                    var playerNewbie = BuildNewbiePlayer();

                    context.Player.Add(playerFull);
                    context.Player.Add(playerNewbie);
                    context.SaveChanges();

                    playerFull.SelectedAvatar_Player_idPlayer = playerFull.idPlayer;
                    playerFull.SelectedAvatar_Avatar_idAvatar = 1;
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception($"FATAL ERROR en SeedDatabase: {ex.Message}", ex);
                }
            }
        }

        private PlayerRepository CreateRepo()
        {
            return new PlayerRepository(() => GetContext());
        }

        private Player BuildFullPlayer(Avatar avatar)
        {
            var p = new Player
            {
                nickname = "TikiTest",
                fullName = "Tiki Tester",
                revoCoins = 100,
                Account = new List<Account>(),
                PlayerStatistics = new List<PlayerStatistics>(),
                SocialNetwork = new List<SocialNetwork>(),
                AvatarsUnlocked = new List<AvatarsUnlocked>()
            };
            p.Account.Add(new Account { email = "tiki@test.com", password = "hashed" });
            p.PlayerStatistics.Add(new PlayerStatistics { wins = 5, matchesPlayed = 10 });
            p.SocialNetwork.Add(new SocialNetwork { tipoRedSocial = "Facebook", linkRedSocial = "fb.com/tiki" });

            if (avatar != null)
                p.AvatarsUnlocked.Add(new AvatarsUnlocked { Avatar = avatar, unlockedDate = DateTime.Now });

            return p;
        }

        private Player BuildNewbiePlayer()
        {
            var p = new Player
            {
                nickname = "Newbie",
                fullName = "Noob Tester",
                revoCoins = 0,
                Account = new List<Account>(),
                PlayerStatistics = new List<PlayerStatistics>(),
                SocialNetwork = new List<SocialNetwork>(),
                AvatarsUnlocked = new List<AvatarsUnlocked>()
            };
            p.Account.Add(new Account { email = "newbie@test.com", password = "hashed" });
            return p;
        }

        [Fact]
        public async Task TestGetPlayerProfileUserExistsReturnsBasicInfoAndAccount()
        {
            var result = await CreateRepo().GetPlayerProfileByNicknameAsync("TikiTest");
            Assert.NotNull(result);
            Assert.Equal("TikiTest", result.nickname);
            Assert.Equal("tiki@test.com", result.Account.First().email);
        }

        [Fact]
        public async Task TestGetPlayerProfileUserExistsReturnsStatsAndSocials()
        {
            var result = await CreateRepo().GetPlayerProfileByNicknameAsync("TikiTest");
            Assert.NotNull(result);
            Assert.Equal(5, result.PlayerStatistics.First().wins);
            Assert.Equal("Facebook", result.SocialNetwork.First().tipoRedSocial);
        }

        [Fact]
        public async Task TestGetPlayerProfileUserExistsReturnsNestedAvatars()
        {
            var result = await CreateRepo().GetPlayerProfileByNicknameAsync("TikiTest");
            Assert.NotNull(result);
            Assert.Equal("Default", result.AvatarsUnlocked.First().Avatar.avatarName);
        }

        [Fact]
        public async Task TestGetPlayerProfileUserDoesNotExistReturnsNull()
        {
            var result = await CreateRepo().GetPlayerProfileByNicknameAsync("GhostUser");
            Assert.Null(result);
        }

        [Fact]
        public async Task TestGetPlayerProfileNewUserWithNoStatsReturnsEmptyListsButNotNull()
        {
            var result = await CreateRepo().GetPlayerProfileByNicknameAsync("Newbie");
            Assert.NotNull(result);
            Assert.True(result.PlayerStatistics == null || result.PlayerStatistics.Count == 0);
        }

        [Fact]
        public async Task TestGetPlayerProfileCaseInsensitiveShouldFindUserUpperCase()
        {
            var result = await CreateRepo().GetPlayerProfileByNicknameAsync("TIKITEST");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TestGetPlayerProfileCaseInsensitiveShouldFindUserLowerCase()
        {
            var result = await CreateRepo().GetPlayerProfileByNicknameAsync("tikitest");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TestGetPlayerProfileWithLeadingSpacesShouldNotMatchExact()
        {
            var result = await CreateRepo().GetPlayerProfileByNicknameAsync(" TikiTest");
            Assert.Null(result);
        }

        [Fact]
        public async Task TestGetPlayerProfileEmptyStringReturnsNull()
        {
            var result = await CreateRepo().GetPlayerProfileByNicknameAsync("");
            Assert.Null(result);
        }

        [Fact]
        public async Task TestGetPlayerProfileNullStringReturnsNull()
        {
            var result = await CreateRepo().GetPlayerProfileByNicknameAsync(null);
            Assert.Null(result);
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncBasicInfoUpdatesDatabase()
        {
            var updateData = new Contracts.DTOs.ProfileData { Nickname = "TikiTest", FullName = "Updated", Email = "u@t.com" };
            await CreateRepo().UpdatePlayerProfileAsync(updateData);

            using (var context = GetContext())
            {
                var p = context.Player.Include("Account").FirstOrDefault(x => x.nickname == "TikiTest");
                Assert.Equal("Updated", p.fullName);
                Assert.Equal("u@t.com", p.Account.First().email);
            }
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncNewPasswordUpdatesHash()
        {
            string oldHash;
            using (var ctx = GetContext()) oldHash = ctx.Account.First(a => a.Player.nickname == "TikiTest").password;

            await CreateRepo().UpdatePlayerProfileAsync(new Contracts.DTOs.ProfileData { Nickname = "TikiTest", Email = "t@t.com", Password = "New!" });

            using (var ctx = GetContext()) Assert.NotEqual(oldHash, ctx.Account.First(a => a.Player.nickname == "TikiTest").password);
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncUpdateExistingSocialNetworkChangesLink()
        {
            await CreateRepo().UpdatePlayerProfileAsync(new Contracts.DTOs.ProfileData { Nickname = "TikiTest", Email = "t@t.com", FacebookUrl = "new.fb" });

            using (var ctx = GetContext())
                Assert.Equal("new.fb", ctx.SocialNetwork.First(s => s.Player.nickname == "TikiTest" && s.tipoRedSocial == "Facebook").linkRedSocial);
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncAddNewSocialNetworkInsertsRecord()
        {
            await CreateRepo().UpdatePlayerProfileAsync(new Contracts.DTOs.ProfileData { Nickname = "TikiTest", Email = "t@t.com", InstagramUrl = "new.ig" });

            using (var ctx = GetContext())
                Assert.Equal("new.ig", ctx.SocialNetwork.First(s => s.Player.nickname == "TikiTest" && s.tipoRedSocial == "Instagram").linkRedSocial);
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncEmptySocialUrlDoesNotDeleteOrError()
        {
            await CreateRepo().UpdatePlayerProfileAsync(new Contracts.DTOs.ProfileData { Nickname = "TikiTest", Email = "t@t.com", FacebookUrl = "" });

            using (var ctx = GetContext())
                Assert.Equal("fb.com/tiki", ctx.SocialNetwork.First(s => s.Player.nickname == "TikiTest" && s.tipoRedSocial == "Facebook").linkRedSocial);
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncNonExistentUserThrowsException()
        {
            await Assert.ThrowsAsync<ValidationException>(() => CreateRepo().UpdatePlayerProfileAsync(new Contracts.DTOs.ProfileData { Nickname = "Ghost" }));
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncFullNameTooLongThrowsWrappedValidationException()
        {
            var data = new Contracts.DTOs.ProfileData { Nickname = "TikiTest", Email = "t@t.com", FullName = new string('A', 300) };

            var ex = await Assert.ThrowsAsync<Exception>(() => CreateRepo().UpdatePlayerProfileAsync(data));
            Assert.Contains("Error de validación", ex.Message);
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncSqlConstraintViolationRollsBackAllChanges()
        {
            var data = new Contracts.DTOs.ProfileData { Nickname = "TikiTest", FullName = "BAD", Email = "newbie@test.com" }; // Duplicado
            await Assert.ThrowsAsync<System.Data.Entity.Infrastructure.DbUpdateException>(() => CreateRepo().UpdatePlayerProfileAsync(data));

            using (var ctx = GetContext())
            {
                var p = ctx.Player.FirstOrDefault(x => x.nickname == "TikiTest");
                if (p != null) Assert.Equal("Tiki Tester", p.fullName);
            }
        }

        [Fact]
        public async Task TestUpdatePlayerProfileAsyncDuplicateEmailThrowsDbUpdateException()
        {
            var data = new Contracts.DTOs.ProfileData { Nickname = "TikiTest", Email = "newbie@test.com" };
            await Assert.ThrowsAsync<System.Data.Entity.Infrastructure.DbUpdateException>(() => CreateRepo().UpdatePlayerProfileAsync(data));
        }

        [Fact]
        public async Task TestCreatePlayerAsyncValidDataInsertsPlayerAndAccount()
        {
            var data = new Contracts.DTOs.RegistrationData { Nickname = "NewReg", FullName = "N", Email = "n@n.com", Password = "P" };
            await CreateRepo().CreatePlayerAsync(data);

            using (var ctx = GetContext())
            {
                var p = ctx.Player.Include("Account").FirstOrDefault(x => x.nickname == "NewReg");
                Assert.NotNull(p);
                Assert.Equal("n@n.com", p.Account.First().email);
            }
        }

        [Fact]
        public async Task TestCreatePlayerAsyncDuplicateNicknameThrowsDbUpdateException()
        {
            var data = new Contracts.DTOs.RegistrationData { Nickname = "TikiTest", FullName = "F", Email = "uniq@u.com", Password = "P" };
            await Assert.ThrowsAsync<System.Data.Entity.Infrastructure.DbUpdateException>(() => CreateRepo().CreatePlayerAsync(data));
        }

        [Fact]
        public async Task TestCreatePlayerAsyncDuplicateEmailThrowsDbUpdateException()
        {
            var data = new Contracts.DTOs.RegistrationData { Nickname = "Uniq", FullName = "F", Email = "tiki@test.com", Password = "P" };
            await Assert.ThrowsAsync<System.Data.Entity.Infrastructure.DbUpdateException>(() => CreateRepo().CreatePlayerAsync(data));
        }

        [Fact]
        public async Task TestIsNicknameTakenAsyncExistingNickReturnsTrue()
        {
            Assert.True(await CreateRepo().IsNicknameTakenAsync("TikiTest"));
        }

        [Fact]
        public async Task TestIsNicknameTakenAsyncNewNickReturnsFalse()
        {
            Assert.False(await CreateRepo().IsNicknameTakenAsync("Free"));
        }

        [Fact]
        public async Task TestIsEmailRegisteredAsyncExistingEmailReturnsTrue()
        {
            Assert.True(await CreateRepo().IsEmailRegisteredAsync("tiki@test.com"));
        }

        [Fact]
        public async Task TestIsEmailRegisteredAsyncNewEmailReturnsFalse()
        {
            Assert.False(await CreateRepo().IsEmailRegisteredAsync("free@t.com"));
        }

        [Fact]
        public async Task TestCreatePlayerFromPendingAsyncWithValidDataInsertsRecord()
        {
            var pending = new UnoLisServer.Common.Models.PendingRegistration { Nickname = "PendingUser", FullName = "P", HashedPassword = "H" };
            await CreateRepo().CreatePlayerFromPendingAsync("pending@test.com", pending);

            using (var ctx = GetContext())
            {
                var p = ctx.Player.Include("AvatarsUnlocked").FirstOrDefault(x => x.nickname == "PendingUser");
                Assert.NotNull(p);
                Assert.Equal(3, p.AvatarsUnlocked.Count);
            }
        }

        [Fact]
        public async Task TestGetPlayerAvatarsAsyncReturnsCorrectListAndSelection()
        {
            var avatars = await CreateRepo().GetPlayerAvatarsAsync("TikiTest");

            Assert.NotNull(avatars);
            Assert.True(avatars.Count >= 3);

            var defaultAvatar = avatars.First(a => a.AvatarId == 1);
            Assert.True(defaultAvatar.IsSelected);
            Assert.True(defaultAvatar.IsUnlocked);

            var lockedAvatar = avatars.First(a => a.AvatarId == 10);
            Assert.False(lockedAvatar.IsUnlocked);
        }

        [Fact]
        public async Task TestUpdateSelectedAvatarAsyncUpdatesDatabase()
        {
            var repo = CreateRepo();

            using (var ctx = GetContext())
            {
                var player = ctx.Player.First(p => p.nickname == "TikiTest");
                player.AvatarsUnlocked.Add(new AvatarsUnlocked
                {
                    Player_idPlayer = player.idPlayer,
                    Avatar_idAvatar = 10,
                    unlockedDate = DateTime.Now
                });
                ctx.SaveChanges();
            }

            await repo.UpdateSelectedAvatarAsync("TikiTest", 10);

            using (var ctx = GetContext())
            {
                var player = ctx.Player.First(p => p.nickname == "TikiTest");
                Assert.Equal(10, player.SelectedAvatar_Avatar_idAvatar);
            }
        }

        [Fact]
        public async Task TestUpdateSelectedAvatarAsyncInvalidUserThrowsException()
        {
            await Assert.ThrowsAsync<ValidationException>(() => CreateRepo().UpdateSelectedAvatarAsync("Ghost", 1));
        }
    }
}