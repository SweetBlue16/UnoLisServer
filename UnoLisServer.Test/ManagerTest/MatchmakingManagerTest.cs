using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Services;
using UnoLisServer.Services.ManagerInterfaces;
using Xunit;

namespace UnoLisServer.Test
{
    public class MatchmakingManagerTest
    {
        private readonly Mock<ILobbyManager> _mockLobbyManager;
        private readonly MatchmakingManager _manager;

        public MatchmakingManagerTest()
        {
            _mockLobbyManager = new Mock<ILobbyManager>();
            _manager = new MatchmakingManager(_mockLobbyManager.Object);
        }

        [Fact]
        public async Task TestCreateMatchAsync_DelegatesToLobbyManager()
        {
            var settings = new MatchSettings();
            var expectedResponse = new CreateMatchResponse { Success = true };

            _mockLobbyManager.Setup(x => x.CreateLobbyAsync(settings))
                             .ReturnsAsync(expectedResponse);

            var result = await _manager.CreateMatchAsync(settings);

            Assert.Same(expectedResponse, result);
            _mockLobbyManager.Verify(x => x.CreateLobbyAsync(settings), Times.Once);
        }

        [Fact]
        public async Task TestJoinMatchAsync_DelegatesToLobbyManager()
        {
            var expectedResponse = new JoinMatchResponse { Success = true };
            _mockLobbyManager.Setup(x => x.JoinLobbyAsync("CODE", "User"))
                             .ReturnsAsync(expectedResponse);

            var result = await _manager.JoinMatchAsync("CODE", "User");

            Assert.Same(expectedResponse, result);
            _mockLobbyManager.Verify(x => x.JoinLobbyAsync("CODE", "User"), Times.Once);
        }

        [Fact]
        public async Task TestSetLobbyBackgroundAsync_DelegatesToLobbyManager()
        {
            _mockLobbyManager.Setup(x => x.SetLobbyBackgroundAsync("CODE", "Video"))
                             .ReturnsAsync(true);

            var result = await _manager.SetLobbyBackgroundAsync("CODE", "Video");

            Assert.True(result);
            _mockLobbyManager.Verify(x => x.SetLobbyBackgroundAsync("CODE", "Video"), Times.Once);
        }

        [Fact]
        public async Task TestSendInvitationsAsync_DelegatesToLobbyManager()
        {
            var list = new List<string> { "A", "B" };
            _mockLobbyManager.Setup(x => x.SendInvitationsAsync("CODE", "Host", list))
                             .ReturnsAsync(true);

            var result = await _manager.SendInvitationsAsync("CODE", "Host", list);

            Assert.True(result);
            _mockLobbyManager.Verify(x => x.SendInvitationsAsync("CODE", "Host", list), Times.Once);
        }

        [Fact]
        public async Task TestGetLobbySettingsAsync_ReturnsSettingsIfFound()
        {
            var expectedSettings = new LobbySettings { BackgroundVideoName = "Test" };

            _mockLobbyManager.Setup(x => x.GetLobbySettings("CODE"))
                             .Returns(expectedSettings);

            var result = await _manager.GetLobbySettingsAsync("CODE");
            Assert.Same(expectedSettings, result);
        }

        [Fact]
        public async Task TestGetLobbySettingsAsync_ReturnsEmptyObjectIfNull()
        {
            _mockLobbyManager.Setup(x => x.GetLobbySettings("CODE"))
                             .Returns((LobbySettings)null);

            var result = await _manager.GetLobbySettingsAsync("CODE");
            Assert.NotNull(result);
            Assert.Null(result.BackgroundVideoName); 
        }
    }
}