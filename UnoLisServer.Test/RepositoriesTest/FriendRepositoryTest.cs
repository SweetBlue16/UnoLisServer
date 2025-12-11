using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Test.Common;
using Xunit;

namespace UnoLisServer.Test
{
    // Este atributo es VITAL para evitar Deadlocks. Obliga a correr secuencialmente.
    [Collection("DatabaseTests")]
    public class FriendRepositoryTest : UnoLisTestBase
    {
        // Variables dinámicas para guardar los IDs reales que genere SQL Server
        private int _idAlpha;
        private int _idBeta;
        private int _idGamma;
        private int _idDelta;

        public FriendRepositoryTest()
        {
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            using (var context = GetContext())
            {
                context.Database.ExecuteSqlCommand("DELETE FROM [dbo].[Account]");
                context.Database.ExecuteSqlCommand("DELETE FROM [dbo].[FriendList]");
                context.Database.ExecuteSqlCommand("DELETE FROM [dbo].[Player]");

                var pAlpha = new Player { nickname = "Alpha", fullName = "Alpha User", revoCoins = 0 };
                var pBeta = new Player { nickname = "Beta", fullName = "Beta User", revoCoins = 0 };
                var pGamma = new Player { nickname = "Gamma", fullName = "Gamma User", revoCoins = 0 };
                var pDelta = new Player { nickname = "Delta", fullName = "Delta User", revoCoins = 0 };

                pAlpha.Account.Add(new Account { email = "a@test.com", password = "pass" });
                pBeta.Account.Add(new Account { email = "b@test.com", password = "pass" });
                pGamma.Account.Add(new Account { email = "g@test.com", password = "pass" });
                pDelta.Account.Add(new Account { email = "d@test.com", password = "pass" });

                context.Player.Add(pAlpha);
                context.Player.Add(pBeta);
                context.Player.Add(pGamma);
                context.Player.Add(pDelta);

                context.SaveChanges();

                _idAlpha = pAlpha.idPlayer;
                _idBeta = pBeta.idPlayer;
                _idGamma = pGamma.idPlayer;
                _idDelta = pDelta.idPlayer;

                context.FriendList.Add(new FriendList { Player_idPlayer = _idAlpha, Player_idPlayer1 = _idBeta, 
                    friendRequest = true });

                context.FriendList.Add(new FriendList { Player_idPlayer = _idGamma, Player_idPlayer1 = _idAlpha,
                    friendRequest = false });

                context.SaveChanges();
            }
        }

        private FriendRepository CreateRepo()
        {
            return new FriendRepository(() => GetContext());
        }

        [Fact]
        public async Task GetFriendsEntities_AsRequester_ReturnsTargetFriend()
        {
            var repo = CreateRepo();
            var friends = await repo.GetFriendsEntitiesAsync("Alpha");
            Assert.NotNull(friends);
            Assert.Contains(friends, f => f.nickname == "Beta");
        }

        [Fact]
        public async Task GetFriendsEntities_AsTarget_ReturnsRequesterFriend()
        {
            var repo = CreateRepo();
            var friends = await repo.GetFriendsEntitiesAsync("Beta");
            Assert.NotNull(friends);
            Assert.Contains(friends, f => f.nickname == "Alpha");
        }

        [Fact]
        public async Task GetFriendsEntities_PendingRequest_IsNotInFriendsList()
        {
            var repo = CreateRepo();
            var friends = await repo.GetFriendsEntitiesAsync("Alpha");
            Assert.DoesNotContain(friends, f => f.nickname == "Gamma");
        }

        [Fact]
        public async Task GetFriendsEntities_NonExistentUser_ReturnsEmptyList()
        {
            var repo = CreateRepo();
            var result = await repo.GetFriendsEntitiesAsync("GhostUser");
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPendingRequests_ReturnsIncomingRequestsOnly()
        {
            var repo = CreateRepo();
            var requests = await repo.GetPendingRequestsEntitiesAsync("Alpha");
            Assert.Single(requests);
            Assert.Equal("Gamma", requests.First().Player.nickname);
        }

        [Fact]
        public async Task GetPendingRequests_OutgoingRequestsAreNotShown()
        {
            var repo = CreateRepo();
            var requests = await repo.GetPendingRequestsEntitiesAsync("Gamma");
            Assert.Empty(requests);
        }

        [Fact]
        public async Task GetPendingRequests_ConfirmedFriendsAreNotShown()
        {
            var repo = CreateRepo();
            var requests = await repo.GetPendingRequestsEntitiesAsync("Beta");
            Assert.Empty(requests);
        }

        [Fact]
        public async Task GetFriendshipEntry_FoundByIds_OrderDoesNotMatter()
        {
            var repo = CreateRepo();
            var rel1 = await repo.GetFriendshipEntryAsync(_idAlpha, _idBeta);
            Assert.NotNull(rel1);

            var rel2 = await repo.GetFriendshipEntryAsync(_idBeta, _idAlpha);
            Assert.NotNull(rel2);
            Assert.Equal(rel1.idFriendList, rel2.idFriendList);
        }

        [Fact]
        public async Task GetFriendshipEntry_NoRelationship_ReturnsNull()
        {
            var repo = CreateRepo();
            var rel = await repo.GetFriendshipEntryAsync(_idAlpha, _idDelta);
            Assert.Null(rel);
        }

        [Fact]
        public async Task CreateFriendRequest_ValidUsers_InsertsPendingRequest()
        {
            var repo = CreateRepo();
            var newReq = await repo.CreateFriendRequestAsync(_idAlpha, _idDelta); 

            using (var ctx = GetContext())
            {
                var entry = ctx.FriendList.Find(newReq.idFriendList);
                Assert.NotNull(entry);
                Assert.False(entry.friendRequest);
                Assert.Equal(_idAlpha, entry.Player_idPlayer);
                Assert.Equal(_idDelta, entry.Player_idPlayer1);
            }
        }

        [Fact]
        public async Task CreateFriendRequest_DuplicateRequest_ThrowsDataConflictException()
        {
            var repo = CreateRepo();
            var ex = await Assert.ThrowsAsync<Exception>(() => repo.CreateFriendRequestAsync(_idAlpha, 9999999));

            Assert.Equal("Data_Conflict", ex.Message);
        }

        [Fact]
        public async Task AcceptFriendRequest_ValidId_UpdatesStatusToTrue()
        {
            var repo = CreateRepo();
            int reqId;
            using (var ctx = GetContext())
            {
                reqId = ctx.FriendList.First(f => f.Player_idPlayer == _idGamma && f.Player_idPlayer1 == 
                _idAlpha).idFriendList;
            }

            await repo.AcceptFriendRequestAsync(reqId);

            using (var ctx = GetContext())
            {
                var entry = ctx.FriendList.Find(reqId);
                Assert.True(entry.friendRequest);
            }
        }

        [Fact]
        public async Task RemoveFriendshipEntry_ValidId_DeletesRecord()
        {
            var repo = CreateRepo();
            int relId;
            using (var ctx = GetContext())
            {
                // Buscamos Alpha<->Beta
                relId = ctx.FriendList.First(f => f.Player_idPlayer == _idAlpha && f.Player_idPlayer1 == _idBeta).idFriendList;
            }

            await repo.RemoveFriendshipEntryAsync(relId);

            using (var ctx = GetContext())
            {
                var entry = ctx.FriendList.Find(relId);
                Assert.Null(entry);
            }
        }

        [Fact]
        public async Task RemoveFriendshipEntry_NonExistentId_DoesNotThrow()
        {
            var repo = CreateRepo();
            await repo.RemoveFriendshipEntryAsync(9999999);
        }
    }
}