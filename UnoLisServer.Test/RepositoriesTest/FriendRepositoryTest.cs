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
    [Collection("DatabaseTests")]
    public class FriendRepositoryTest : UnoLisTestBase
    {
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
        public async Task TestGetFriendsEntitiesAsRequesterReturnsTargetFriend()
        {
            var repo = CreateRepo();
            var friends = await repo.GetFriendsEntitiesAsync("Alpha");
            Assert.Contains(friends, f => f.nickname == "Beta");
        }

        [Fact]
        public async Task TestGetFriendsEntitiesAsTargetReturnsRequesterFriend()
        {
            var repo = CreateRepo();
            var friends = await repo.GetFriendsEntitiesAsync("Beta");
            Assert.Contains(friends, f => f.nickname == "Alpha");
        }

        [Fact]
        public async Task TestGetFriendsEntitiesPendingRequestIsNotInFriendsList()
        {
            var repo = CreateRepo();
            var friends = await repo.GetFriendsEntitiesAsync("Alpha");
            Assert.DoesNotContain(friends, f => f.nickname == "Gamma");
        }

        [Fact]
        public async Task TestGetFriendsEntitiesNonExistentUserReturnsEmptyList()
        {
            var repo = CreateRepo();
            var result = await repo.GetFriendsEntitiesAsync("GhostUser");
            Assert.Empty(result);
        }

        [Fact]
        public async Task TestGetPendingRequestsReturnsIncomingRequestsOnly()
        {
            var repo = CreateRepo();
            var requests = await repo.GetPendingRequestsEntitiesAsync("Alpha");
            Assert.Single(requests);
            Assert.Equal("Gamma", requests.First().Player.nickname);
        }

        [Fact]
        public async Task TestGetPendingRequestsOutgoingRequestsAreNotShown()
        {
            var repo = CreateRepo();
            var requests = await repo.GetPendingRequestsEntitiesAsync("Gamma");
            Assert.Empty(requests);
        }

        [Fact]
        public async Task TestGetPendingRequestsConfirmedFriendsAreNotShown()
        {
            var repo = CreateRepo();
            var requests = await repo.GetPendingRequestsEntitiesAsync("Beta");
            Assert.Empty(requests);
        }

        [Fact]
        public async Task TestGetFriendshipEntryFoundByIdsOrderDoesNotMatter()
        {
            var repo = CreateRepo();
            var rel1 = await repo.GetFriendshipEntryAsync(_idAlpha, _idBeta);

            var rel2 = await repo.GetFriendshipEntryAsync(_idBeta, _idAlpha);
            Assert.Equal(rel1.idFriendList, rel2.idFriendList);
        }

        [Fact]
        public async Task TestGetFriendshipEntryNoRelationshipReturnsNull()
        {
            var repo = CreateRepo();
            var rel = await repo.GetFriendshipEntryAsync(_idAlpha, _idDelta);
            Assert.Null(rel);
        }

        [Fact]
        public async Task TestCreateFriendRequestValidUsersInsertsPendingRequest()
        {
            var repo = CreateRepo();
            var newReq = await repo.CreateFriendRequestAsync(_idAlpha, _idDelta); 

            using (var ctx = GetContext())
            {
                var entry = ctx.FriendList.Find(newReq.idFriendList);
                Assert.False(entry.friendRequest);
                Assert.Equal(_idAlpha, entry.Player_idPlayer);
                Assert.Equal(_idDelta, entry.Player_idPlayer1);
            }
        }

        [Fact]
        public async Task TestCreateFriendRequestDuplicateRequestThrowsDataConflictException()
        {
            var repo = CreateRepo();
            var ex = await Assert.ThrowsAsync<Exception>(() => repo.CreateFriendRequestAsync(_idAlpha, 9999999));

            Assert.Equal("Data_Conflict", ex.Message);
        }

        [Fact]
        public async Task TestAcceptFriendRequestValidIdUpdatesStatusToTrue()
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
        public async Task TestRemoveFriendshipEntryValidIdDeletesRecord()
        {
            var repo = CreateRepo();
            int relId;
            using (var ctx = GetContext())
            {
                relId = ctx.FriendList.First(f => f.Player_idPlayer == _idAlpha && 
                f.Player_idPlayer1 == _idBeta).idFriendList;
            }

            await repo.RemoveFriendshipEntryAsync(relId);

            using (var ctx = GetContext())
            {
                var entry = ctx.FriendList.Find(relId);
                Assert.Null(entry);
            }
        }

        [Fact]
        public async Task TestRemoveFriendshipEntryNonExistentIdDoesNotThrow()
        {
            var repo = CreateRepo();
            await repo.RemoveFriendshipEntryAsync(9999999);
        }
    }
}