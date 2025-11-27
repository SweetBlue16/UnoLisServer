using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Contracts.DTOs; // Para LeaderboardEntry
using UnoLisServer.Data; // Para PlayerStatistics y Player
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
        public async Task GetGlobalLeaderboard_ReturnsDataSuccessfully()
        {
            // Arrange
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

            // Act
            var response = await _manager.GetGlobalLeaderboardAsync();

            // Assert
            Assert.True(response.Success);
            Assert.Equal(MessageCode.LeaderboardDataRetrieved, response.Code);
            Assert.NotNull(response.Data);
            Assert.Equal(2, response.Data.Count);

            // Validar mapeo y cálculo
            var first = response.Data[0];
            Assert.Equal(1, first.Rank);
            Assert.Equal("ProPlayer", first.Nickname);
            Assert.Equal("50%", first.WinRate); // 5/10

            var second = response.Data[1];
            Assert.Equal(2, second.Rank);
            Assert.Equal("0%", second.WinRate); // 0/5
        }

        [Fact]
        public async Task GetGlobalLeaderboard_EmptyList_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetTopPlayersByGlobalScoreAsync(It.IsAny<int>()))
                     .ReturnsAsync(new List<PlayerStatistics>());

            // Act
            var response = await _manager.GetGlobalLeaderboardAsync();

            // Assert
            Assert.True(response.Success);
            Assert.Equal(MessageCode.Success, response.Code);
            Assert.Empty(response.Data);
        }

        [Fact]
        public async Task GetGlobalLeaderboard_NullList_ReturnsSuccessWithEmptyData()
        {
            // Arrange - Caso defensivo
            _mockRepo.Setup(r => r.GetTopPlayersByGlobalScoreAsync(It.IsAny<int>()))
                     .ReturnsAsync((List<PlayerStatistics>)null);

            // Act
            var response = await _manager.GetGlobalLeaderboardAsync();

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Empty(response.Data);
        }

        [Fact]
        public async Task GetGlobalLeaderboard_DbError_ReturnsInternalError()
        {
            // Arrange
            // Usamos el SqlExceptionBuilder que ya creaste
            _mockRepo.Setup(r => r.GetTopPlayersByGlobalScoreAsync(It.IsAny<int>()))
                     .ThrowsAsync(SqlExceptionBuilder.Build());

            // Act
            var response = await _manager.GetGlobalLeaderboardAsync();

            // Assert
            Assert.False(response.Success);
            // Tu código actual atrapa SqlException pero la maneja en el bloque genérico Exception
            // o en el bloque SqlException si lo agregaste (recomendado).
            // El resultado será LeaderboardInternalError en ambos casos.
            Assert.Equal(MessageCode.LeaderboardInternalError, response.Code);
        }

        [Fact]
        public async Task GetGlobalLeaderboard_Timeout_ReturnsTimeoutCode()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetTopPlayersByGlobalScoreAsync(It.IsAny<int>()))
                     .ThrowsAsync(new TimeoutException("DB slow"));

            // Act
            var response = await _manager.GetGlobalLeaderboardAsync();

            // Assert
            Assert.False(response.Success);
            Assert.Equal(MessageCode.Timeout, response.Code);
        }

        [Fact]
        public async Task GetGlobalLeaderboard_CalculateWinRate_HandlesZeroMatches()
        {
            // Arrange
            var statsList = new List<PlayerStatistics>
            {
                new PlayerStatistics
                {
                    Player = new Player { nickname = "ZeroUser" },
                    matchesPlayed = 0, // División por cero potencial
                    wins = 0
                }
            };

            _mockRepo.Setup(r => r.GetTopPlayersByGlobalScoreAsync(It.IsAny<int>()))
                     .ReturnsAsync(statsList);

            // Act
            var response = await _manager.GetGlobalLeaderboardAsync();

            // Assert
            Assert.True(response.Success);
            Assert.Equal("0%", response.Data[0].WinRate); // Debe manejarlo gracefully
        }
    }
}