using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Models;
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services;
using UnoLisServer.Services.Helpers;
using UnoLisServer.Test.Common;
using Xunit;

namespace UnoLisServer.Test
{
    public class LobbyManagerTest : IDisposable
    {
        private readonly Mock<IPlayerRepository> _mockRepo;
        private readonly Mock<ILobbyInvitationHelper> _mockInvitationHelper;
        private readonly LobbyManager _manager;
        private readonly LobbySessionHelper _sessionHelper;

        public LobbyManagerTest()
        {
            _mockRepo = new Mock<IPlayerRepository>();
            _mockInvitationHelper = new Mock<ILobbyInvitationHelper>();
            _sessionHelper = LobbySessionHelper.Instance;

            _manager = new LobbyManager(_sessionHelper, _mockRepo.Object, _mockInvitationHelper.Object);
        }

        public void Dispose()
        {
        }

        [Fact]
        public async Task TestCreateLobby_ValidSettings_ReturnsSuccess()
        {
            var settings = new MatchSettings { HostNickname = "Host", MaxPlayers = 4 };
            SetupMockAvatar("Host", "Avatar1");

            var response = await _manager.CreateLobbyAsync(settings);

            Assert.True(response.Success);
            Assert.NotNull(response.LobbyCode);
            Assert.NotNull(_sessionHelper.GetLobby(response.LobbyCode));
        }

        [Fact]
        public async Task TestCreateLobby_NullSettings_ReturnsFailureWithMessage()
        {
            var response = await _manager.CreateLobbyAsync(null);

            Assert.False(response.Success);
            Assert.Contains("cannot be null", response.Message);
        }

        [Fact]
        public async Task TestCreateLobby_InvalidMaxPlayers_ReturnsFailure()
        {
            var settings = new MatchSettings { HostNickname = "Host", MaxPlayers = 10 }; 

            var response = await _manager.CreateLobbyAsync(settings);

            Assert.False(response.Success);
            Assert.Contains("between 2 and 4", response.Message);
        }

        [Fact]
        public async Task TestCreateLobby_DbFailureOnAvatar_ReturnsServiceUnavailable()
        {
            var settings = new MatchSettings { HostNickname = "Host", MaxPlayers = 2 };
            _mockRepo.Setup(r => r.GetPlayerWithDetailsAsync(It.IsAny<string>()))
                     .ThrowsAsync(SqlExceptionBuilder.Build());

            var response = await _manager.CreateLobbyAsync(settings);

            Assert.False(response.Success);
            Assert.Contains("Database error", response.Message);
        }

        [Fact]
        public async Task TestCreateLobby_GeneralException_ReturnsInternalError()
        {
            var settings = new MatchSettings { HostNickname = "Host", MaxPlayers = 2 };
            _mockRepo.Setup(r => r.GetPlayerWithDetailsAsync(It.IsAny<string>()))
                     .ThrowsAsync(new Exception("Unexpected Crash"));

            var response = await _manager.CreateLobbyAsync(settings);

            Assert.False(response.Success);
            Assert.Equal("Internal server error.", response.Message);
        }

        [Fact]
        public async Task TestJoinLobby_ValidCodeAndSpace_ReturnsSuccess()
        {
            string code = await CreateLobbyHelper("Host", 4);
            SetupMockAvatar("Joiner", "Avatar2");

            var response = await _manager.JoinLobbyAsync(code, "Joiner");

            Assert.True(response.Success);
            Assert.Equal(2, _sessionHelper.GetLobby(code).Players.Count);
        }

        [Fact]
        public async Task TestJoinLobby_LobbyNotFound_ReturnsFailure()
        {
            var response = await _manager.JoinLobbyAsync("NONEXISTENT", "User");

            Assert.False(response.Success);
            Assert.Equal("Lobby not found.", response.Message);
        }

        [Fact]
        public async Task TestJoinLobby_LobbyFull_ReturnsFailure()
        {
            string code = await CreateLobbyHelper("Host", 2);
            SetupMockAvatar("P2", "A2");
            await _manager.JoinLobbyAsync(code, "P2");

            SetupMockAvatar("P3", "A3");

            var response = await _manager.JoinLobbyAsync(code, "P3"); 

            Assert.False(response.Success);
            Assert.Contains("full", response.Message.ToLower());
        }

        [Fact]
        public async Task TestJoinLobby_InvalidInput_ReturnsFailure()
        {
            var response = await _manager.JoinLobbyAsync("", "User");
            Assert.False(response.Success);
        }

        [Fact]
        public async Task TestJoinLobby_DbFailure_ReturnsServiceUnavailable()
        {
            string code = await CreateLobbyHelper("Host", 4);
            _mockRepo.Setup(r => r.GetPlayerWithDetailsAsync("Joiner"))
                     .ThrowsAsync(SqlExceptionBuilder.Build());

            var response = await _manager.JoinLobbyAsync(code, "Joiner");

            Assert.False(response.Success);
            Assert.Contains("Database error", response.Message);
        }

        [Fact]
        public async Task TestSetLobbyBackground_ExistingLobby_ReturnsTrue()
        {
            string code = await CreateLobbyHelper("Host", 2);

            bool result = await _manager.SetLobbyBackgroundAsync(code, "Space");

            Assert.True(result);
            Assert.Equal("Space", _sessionHelper.GetLobby(code).SelectedBackgroundVideo);
        }

        [Fact]
        public async Task TestSetLobbyBackground_NonExistent_ReturnsFalse()
        {
            bool result = await _manager.SetLobbyBackgroundAsync("FAKE", "Space");
            Assert.False(result);
        }

        [Fact]
        public async Task TestSetLobbyBackground_EmptyName_ReturnsFalse()
        {
            string code = await CreateLobbyHelper("Host", 2);
            bool result = await _manager.SetLobbyBackgroundAsync(code, "");
            Assert.False(result);
        }

        [Fact]
        public async Task TestGetLobbySettings_ValidCode_ReturnsSettings()
        {
            string code = await CreateLobbyHelper("Host", 2);
            await _manager.SetLobbyBackgroundAsync(code, "Volcano");

            var settings = _manager.GetLobbySettings(code);

            Assert.NotNull(settings);
            Assert.Equal("Volcano", settings.BackgroundVideoName);
        }

        [Fact]
        public void TestGetLobbySettings_InvalidCode_ReturnsNull()
        {
            var result = _manager.GetLobbySettings("FAKE");
            Assert.Null(result);
        }

        [Fact]
        public async Task TestSendInvitations_DelegatesToHelper_ReturnsHelperResult()
        {
            string code = "CODE1";
            var list = new List<string> { "Friend" };
            _mockInvitationHelper.Setup(h => h.SendInvitationsAsync(code, "Host", list)).ReturnsAsync(true);

            var result = await _manager.SendInvitationsAsync(code, "Host", list);

            Assert.True(result);
            _mockInvitationHelper.Verify(h => h.SendInvitationsAsync(code, "Host", list), Times.Once);
        }

        [Fact]
        public async Task TestHandleReadyStatus_UpdatesPlayerState()
        {
            string code = await CreateLobbyHelper("Host", 2);
            SetupMockAvatar("P2", "A");
            await _manager.JoinLobbyAsync(code, "P2");

            await _manager.HandleReadyStatusAsync(code, "P2", true);

            var lobby = _sessionHelper.GetLobby(code);
            var player = lobby.Players.Find(p => p.Nickname == "P2");
            Assert.True(player.IsReady);
        }

        [Fact]
        public async Task TestHandleReadyStatus_NonExistentLobby_DoesNotThrow()
        {
            await _manager.HandleReadyStatusAsync("FAKE", "User", true);
        }

        [Fact]
        public async Task TestHandleReadyStatus_WhenAllReady_TriggerGameLogic()
        {
            string code = await CreateLobbyHelper("Host", 2);
            SetupMockAvatar("P2", "A");
            await _manager.JoinLobbyAsync(code, "P2");

            await _manager.HandleReadyStatusAsync(code, "Host", true);
            await _manager.HandleReadyStatusAsync(code, "P2", true);

            var lobby = _sessionHelper.GetLobby(code);
            Assert.True(lobby.Players.TrueForAll(p => p.IsReady));
        }

        private async Task<string> CreateLobbyHelper(string host, int max)
        {
            SetupMockAvatar(host, "Default");
            var result = await _manager.CreateLobbyAsync(new MatchSettings { HostNickname = host, MaxPlayers = max });
            return result.LobbyCode;
        }

        private void SetupMockAvatar(string nickname, string avatarName)
        {
            var player = new Player
            {
                nickname = nickname,
                SelectedAvatar_Avatar_idAvatar = 1,
                AvatarsUnlocked = new List<AvatarsUnlocked>
                {
                    new AvatarsUnlocked
                    {
                        Avatar_idAvatar = 1,
                        Avatar = new Avatar { avatarName = avatarName }
                    }
                }
            };
            _mockRepo.Setup(r => r.GetPlayerWithDetailsAsync(nickname)).ReturnsAsync(player);
        }
    }
}