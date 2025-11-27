using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Data;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services;
using UnoLisServer.Test.Common;
using Xunit;

namespace UnoLisServer.Test
{
    public class LeaderboardsManagerTest
    {
        private readonly Mock<IPlayerRepository> _mockRepo;
        private readonly LeaderboardsManager _manager;

        public LeaderboardsManagerTest()
        {
            _mockRepo = new Mock<IPlayerRepository>();
            _manager = new LeaderboardsManager(_mockRepo.Object);
        }

        [Fact]
        public async Task TestGetGlobalLeaderboardReturnsDataSuccessfully()
        {
            var statsList = new List<PlayerStatistics>
            {
                new PlayerStatistics
                {
                    Player = new Player { nickname = "ProPlayer" },
                    globalPoints = 1000,
                    matchesPlayed = 10,
                    wins = 5
                },
                new PlayerStatistics
                {
                    Player = new Player { nickname = "Newbie" },
                    globalPoints = 100,
                    matchesPlayed = 5,
                    wins = 0
                }
            };

            _mockRepo.Setup(r => r.GetTopPlayersByGlobalScoreAsync(It.IsAny<int>()))
                     .ReturnsAsync(statsList);

            var response = await _manager.GetGlobalLeaderboardAsync();

            Assert.True(response.Success);
            Assert.Equal(MessageCode.LeaderboardDataRetrieved, response.Code);
            Assert.NotNull(response.Data);
            Assert.Equal(2, response.Data.Count);

            var first = response.Data[0];
            Assert.Equal(1, first.Rank);
            Assert.Equal("ProPlayer", first.Nickname);
            Assert.Equal("50%", first.WinRate); 

            var second = response.Data[1];
            Assert.Equal(2, second.Rank);
            Assert.Equal("0%", second.WinRate); 
        }

        [Fact]
        public async Task TestGetGlobalLeaderboardEmptyListReturnsSuccessWithEmptyData()
        {
            _mockRepo.Setup(r => r.GetTopPlayersByGlobalScoreAsync(It.IsAny<int>()))
                     .ReturnsAsync(new List<PlayerStatistics>());

            var response = await _manager.GetGlobalLeaderboardAsync();

            Assert.True(response.Success);
            Assert.Equal(MessageCode.Success, response.Code);
            Assert.Empty(response.Data);
        }

        [Fact]
        public async Task TestGetGlobalLeaderboardNullListReturnsSuccessWithEmptyData()
        {
            _mockRepo.Setup(r => r.GetTopPlayersByGlobalScoreAsync(It.IsAny<int>()))
                     .ReturnsAsync((List<PlayerStatistics>)null);

            var response = await _manager.GetGlobalLeaderboardAsync();

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Empty(response.Data);
        }

        [Fact]
        public async Task TestGetGlobalLeaderboardDbErrorReturnsInternalError()
        {
            _mockRepo.Setup(r => r.GetTopPlayersByGlobalScoreAsync(It.IsAny<int>()))
                     .ThrowsAsync(SqlExceptionBuilder.Build());

            var response = await _manager.GetGlobalLeaderboardAsync();

            Assert.False(response.Success);
            Assert.Equal(MessageCode.LeaderboardInternalError, response.Code);
        }

        [Fact]
        public async Task TestGetGlobalLeaderboardTimeoutReturnsTimeoutCode()
        {
            _mockRepo.Setup(r => r.GetTopPlayersByGlobalScoreAsync(It.IsAny<int>()))
                     .ThrowsAsync(new TimeoutException("DB slow"));

            var response = await _manager.GetGlobalLeaderboardAsync();

            Assert.False(response.Success);
            Assert.Equal(MessageCode.Timeout, response.Code);
        }

        [Fact]
        public async Task TestGetGlobalLeaderboardCalculateWinRateHandlesZeroMatches()
        {
            var statsList = new List<PlayerStatistics>
            {
                new PlayerStatistics
                {
                    Player = new Player { nickname = "ZeroUser" },
                    matchesPlayed = 0,
                    wins = 0
                }
            };

            _mockRepo.Setup(r => r.GetTopPlayersByGlobalScoreAsync(It.IsAny<int>()))
                     .ReturnsAsync(statsList);

            var response = await _manager.GetGlobalLeaderboardAsync();

            Assert.True(response.Success);
            Assert.Equal("0%", response.Data[0].WinRate);
        }
    }
}