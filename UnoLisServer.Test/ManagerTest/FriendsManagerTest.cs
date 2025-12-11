using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services;
using UnoLisServer.Test.Common;
using Xunit;

namespace UnoLisServer.Test
{
    public class FriendsManagerTest
    {
        private readonly Mock<IFriendRepository> _mockRepo;
        private readonly Mock<IFriendsCallback> _mockCallback;

        private readonly FriendsManager _manager;

        public FriendsManagerTest()
        {
            _mockRepo = new Mock<IFriendRepository>();
            _mockCallback = new Mock<IFriendsCallback>();

            _mockCallback.As<IContextChannel>()
                .Setup(x => x.State)
                .Returns(CommunicationState.Opened);


            _manager = new FriendsManager(_mockRepo.Object, _mockCallback.Object);
        }

        [Fact]
        public async Task TestSendFriendRequestSuccessReturnsSuccessAndNotifies()
        {
            string p1 = "Alpha";
            string p2 = "Beta";
            var player1 = new Player { idPlayer = 1, nickname = p1 };
            var player2 = new Player { idPlayer = 2, nickname = p2 };

            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync(p1)).ReturnsAsync(player1);
            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync(p2)).ReturnsAsync(player2);

            _mockRepo.Setup(r => r.GetFriendshipEntryAsync(1, 2)).ReturnsAsync((FriendList)null);

            _mockRepo.Setup(r => r.CreateFriendRequestAsync(1, 2))
                .ReturnsAsync(new FriendList { idFriendList = 100, Player_idPlayer = 1, Player_idPlayer1 = 2 });

            _mockCallback.Setup(cb => cb.FriendRequestReceived(It.IsAny<FriendRequestData>()));

            var result = await _manager.SendFriendRequestAsync(p1, p2);

            Assert.Equal(FriendRequestResult.Success, result);
            _mockRepo.Verify(r => r.CreateFriendRequestAsync(1, 2), Times.Once);
        }

        [Fact]
        public async Task TestSendFriendRequestSelfRequestReturnsCannotAddSelf()
        {
            var result = await _manager.SendFriendRequestAsync("Alpha", "Alpha");

            Assert.Equal(FriendRequestResult.CannotAddSelf, result);
            _mockRepo.Verify(r => r.CreateFriendRequestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task TestSendFriendRequestUserNotFoundReturnsUserNotFound()
        {
            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync("Ghost")).ReturnsAsync((Player)null);

            var result = await _manager.SendFriendRequestAsync("Alpha", "Ghost");

            Assert.Equal(FriendRequestResult.UserNotFound, result);
        }

        [Fact]
        public async Task TestSendFriendRequestAlreadyFriendsReturnsAlreadyFriends()
        {
            var p1 = new Player { idPlayer = 1 };
            var p2 = new Player { idPlayer = 2 };
            var existingRel = new FriendList { friendRequest = true };

            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync("A")).ReturnsAsync(p1);
            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync("B")).ReturnsAsync(p2);
            _mockRepo.Setup(r => r.GetFriendshipEntryAsync(1, 2)).ReturnsAsync(existingRel);

            var result = await _manager.SendFriendRequestAsync("A", "B");

            Assert.Equal(FriendRequestResult.AlreadyFriends, result);
        }

        [Fact]
        public async Task TestSendFriendRequestRequestAlreadySentReturnsRequestAlreadySent()
        {
            var existingRel = new FriendList { friendRequest = false, Player_idPlayer = 1 };

            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync("A")).ReturnsAsync(new Player { idPlayer = 1 });
            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync("B")).ReturnsAsync(new Player { idPlayer = 2 });
            _mockRepo.Setup(r => r.GetFriendshipEntryAsync(1, 2)).ReturnsAsync(existingRel);

            var result = await _manager.SendFriendRequestAsync("A", "B");

            Assert.Equal(FriendRequestResult.RequestAlreadySent, result);
        }

        [Fact]
        public async Task TestSendFriendRequestDbExplodesReturnsFailedAndLogs()
        {
            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync(It.IsAny<string>()))
                .ThrowsAsync(SqlExceptionBuilder.Build());

            var result = await _manager.SendFriendRequestAsync("A", "B");

            Assert.Equal(FriendRequestResult.Failed, result);
        }

        [Fact]
        public async Task TestAcceptRequestValidRequestReturnsTrueAndUpdatesDb()
        {
            var requestDto = new FriendRequestData { RequesterNickname = "A", TargetNickname = "B" };
            var p1 = new Player { idPlayer = 1 };
            var p2 = new Player { idPlayer = 2 };
            var pendingRel = new FriendList { idFriendList = 50, friendRequest = false };

            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync("A")).ReturnsAsync(p1);
            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync("B")).ReturnsAsync(p2);
            _mockRepo.Setup(r => r.GetFriendshipEntryAsync(1, 2)).ReturnsAsync(pendingRel);

            var result = await _manager.AcceptFriendRequestAsync(requestDto);

            Assert.True(result);
            _mockRepo.Verify(r => r.AcceptFriendRequestAsync(50), Times.Once);
        }

        [Fact]
        public async Task TestAcceptRequestNoRelationshipFoundReturnsFalse()
        {
            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync(It.IsAny<string>())).ReturnsAsync(new Player());
            _mockRepo.Setup(r => r.GetFriendshipEntryAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((FriendList)null);
            var result = await _manager.AcceptFriendRequestAsync(new FriendRequestData { RequesterNickname = "A", TargetNickname = "B" });

            Assert.False(result);
            _mockRepo.Verify(r => r.AcceptFriendRequestAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task TestAcceptRequestDbErrorReturnsFalse()
        {
            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync(It.IsAny<string>())).ThrowsAsync(new Exception("DB Boom"));
            var result = await _manager.AcceptFriendRequestAsync(new FriendRequestData { RequesterNickname = "A", TargetNickname = "B" });

            Assert.False(result);
        }

        [Fact]
        public async Task TestRejectRequestValidRemovesEntry()
        {
            var requestDto = new FriendRequestData { RequesterNickname = "A", TargetNickname = "B" };
            var rel = new FriendList { idFriendList = 99 };

            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync(It.IsAny<string>())).ReturnsAsync(new Player { idPlayer = 1 });
            _mockRepo.Setup(r => r.GetFriendshipEntryAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(rel);
            var result = await _manager.RejectFriendRequestAsync(requestDto);

            Assert.True(result);
            _mockRepo.Verify(r => r.RemoveFriendshipEntryAsync(99), Times.Once);
        }

        [Fact]
        public async Task TestRemoveFriendIfFriendsRemovesEntry()
        {
            var requestDto = new FriendRequestData { RequesterNickname = "A", TargetNickname = "B" };
            var rel = new FriendList { idFriendList = 77, friendRequest = true };

            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync(It.IsAny<string>())).ReturnsAsync(new Player { idPlayer = 1 });
            _mockRepo.Setup(r => r.GetFriendshipEntryAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(rel);

            var result = await _manager.RemoveFriendAsync(requestDto);

            Assert.True(result);
            _mockRepo.Verify(r => r.RemoveFriendshipEntryAsync(77), Times.Once);
        }

        [Fact]
        public async Task TestRemoveFriendIfNotFriendsReturnsFalseAndDoesNotRemove()
        {
            var requestDto = new FriendRequestData { RequesterNickname = "A", TargetNickname = "B" };
            var rel = new FriendList { idFriendList = 77, friendRequest = false };

            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync(It.IsAny<string>())).ReturnsAsync(new Player { idPlayer = 1 });
            _mockRepo.Setup(r => r.GetFriendshipEntryAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(rel);

            var result = await _manager.RemoveFriendAsync(requestDto);

            Assert.False(result);
            _mockRepo.Verify(r => r.RemoveFriendshipEntryAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task TestGetPendingRequestsReturnsMappedList()
        {
            var pendingList = new List<FriendList>
            {
                new FriendList { idFriendList = 1, Player = new Player { nickname = "Requester1" } },
                new FriendList { idFriendList = 2, Player = new Player { nickname = "Requester2" } }
            };

            _mockRepo.Setup(r => r.GetPendingRequestsEntitiesAsync("Target")).ReturnsAsync(pendingList);
            var result = await _manager.GetPendingRequestsAsync("Target");

            Assert.Equal(2, result.Count);
            Assert.Equal("Requester1", result[0].RequesterNickname);
            Assert.Equal("Target", result[0].TargetNickname);
        }

        [Fact]
        public async Task TestGetFriendsListReturnsMappedList()
        {
            var friendsList = new List<Player>
            {
                new Player { nickname = "Friend1" }
            };

            _mockRepo.Setup(r => r.GetFriendsEntitiesAsync("Me")).ReturnsAsync(friendsList);

            var result = await _manager.GetFriendsListAsync("Me");

            Assert.Single(result);
            Assert.Equal("Friend1", result[0].FriendNickname);
            Assert.Equal("Friend", result[0].StatusMessage);
        }

        [Fact]
        public async Task TestRejectRequestDbErrorReturnsFalse()
        {
            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync(It.IsAny<string>())).ReturnsAsync(new Player { idPlayer = 1 });
            _mockRepo.Setup(r => r.GetFriendshipEntryAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("DB Down"));

            var result = await _manager.RejectFriendRequestAsync(new FriendRequestData { RequesterNickname = "A", TargetNickname = "B" });

            Assert.False(result);
        }

        [Fact]
        public async Task TestRemoveFriendDbErrorReturnsFalse()
        {
            _mockRepo.Setup(r => r.GetPlayerByNicknameAsync(It.IsAny<string>())).ReturnsAsync(new Player { idPlayer = 1 });
            _mockRepo.Setup(r => r.GetFriendshipEntryAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("DB Critical Failure"));

            var result = await _manager.RemoveFriendAsync(new FriendRequestData { RequesterNickname = "A", TargetNickname = "B" });

            Assert.False(result);
        }

        [Fact]
        public async Task TestSendFriendRequestInvalidNicknamesThrowsOrReturnsFailed()
        {
            string valid = "ValidUser";
            string invalid = null;
            var result = await _manager.SendFriendRequestAsync(valid, invalid);

            Assert.Equal(FriendRequestResult.Failed, result);
        }

        [Fact]
        public async Task TestGetFriendsListTimeoutReturnsEmptyList()
        {
            _mockRepo.Setup(r => r.GetFriendsEntitiesAsync(It.IsAny<string>()))
                .ThrowsAsync(new TimeoutException("Too slow"));

            var result = await _manager.GetFriendsListAsync("SlowUser");

            Assert.NotNull(result);
            Assert.Empty(result); 
        }
    }
}