using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.Models; 
using UnoLisServer.Data; 
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services.Helpers;
using UnoLisServer.Test.Common;
using Xunit;

namespace UnoLisServer.Test
{
    public class LobbyInvitationHelperTest
    {
        private readonly Mock<IPlayerRepository> _mockRepo;
        private readonly Mock<INotificationSender> _mockSender;
        private readonly LobbyInvitationHelper _helper;

        public LobbyInvitationHelperTest()
        {
            _mockRepo = new Mock<IPlayerRepository>();
            _mockSender = new Mock<INotificationSender>();
            _helper = new LobbyInvitationHelper(_mockRepo.Object, _mockSender.Object);
        }

        [Fact]
        public async Task SendInvitations_ValidUsersWithEmails_ReturnsTrueAndSends()
        {
            string code = "LOBBY";
            string sender = "Host";
            var nicks = new List<string> { "User1", "User2" };

            SetupMockPlayer("User1", "u1@test.com");
            SetupMockPlayer("User2", "u2@test.com");

            _mockSender.Setup(s => s.SendMatchInvitationAsync(It.IsAny<string>(), sender, code))
                       .Returns(Task.CompletedTask);

            var result = await _helper.SendInvitationsAsync(code, sender, nicks);

            Assert.True(result);
            _mockSender.Verify(s => s.SendMatchInvitationAsync("u1@test.com", sender, code), Times.Once);
            _mockSender.Verify(s => s.SendMatchInvitationAsync("u2@test.com", sender, code), Times.Once);
        }

        [Fact]
        public async Task SendInvitations_NullOrEmptyList_ReturnsFalseImmediately()
        {
            Assert.False(await _helper.SendInvitationsAsync("Code", "Host", null));
            Assert.False(await _helper.SendInvitationsAsync("Code", "Host", new List<string>()));

            _mockRepo.Verify(r => r.GetPlayerWithDetailsAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendInvitations_UsersNotFoundOrNoEmail_ReturnsFalseAndNoSends()
        {
            var nicks = new List<string> { "Ghost", "NoEmailUser" };
            _mockRepo.Setup(r => r.GetPlayerWithDetailsAsync("Ghost")).ReturnsAsync((Player)null);

            var noEmailPlayer = new Player { nickname = "NoEmailUser", Account = new List<Account>() };
            _mockRepo.Setup(r => r.GetPlayerWithDetailsAsync("NoEmailUser")).ReturnsAsync(noEmailPlayer);
            var result = await _helper.SendInvitationsAsync("Code", "Host", nicks);
            Assert.False(result);
            _mockSender.Verify(s => s.SendMatchInvitationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendInvitations_RepositoryThrowsException_ReturnsFalse()
        {
            var nicks = new List<string> { "User1" };
            _mockRepo.Setup(r => r.GetPlayerWithDetailsAsync(It.IsAny<string>()))
                     .ThrowsAsync(new Exception("DB Crash"));

            var result = await _helper.SendInvitationsAsync("Code", "Host", nicks);
            Assert.False(result);
        }

        [Fact]
        public async Task SendInvitations_SenderThrowsException_ReturnsFalse()
        {
            var nicks = new List<string> { "User1" };
            SetupMockPlayer("User1", "valid@test.com");

            _mockSender.Setup(s => s.SendMatchInvitationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                       .ThrowsAsync(new Exception("SMTP Error"));

            var result = await _helper.SendInvitationsAsync("Code", "Host", nicks);
            Assert.False(result); 
        }

        private void SetupMockPlayer(string nickname, string email)
        {
            var p = new Player
            {
                nickname = nickname,
                Account = new List<Account> { new Account { email = email } }
            };
            _mockRepo.Setup(r => r.GetPlayerWithDetailsAsync(nickname)).ReturnsAsync(p);
        }

        [Fact]
        public async Task SendInvitations_RepositoryTimeout_ReturnsFalse()
        {
            var nicks = new List<string> { "User1" };
            _mockRepo.Setup(r => r.GetPlayerWithDetailsAsync(It.IsAny<string>()))
                     .ThrowsAsync(new TimeoutException("DB Timeout"));

            var result = await _helper.SendInvitationsAsync("Code", "Host", nicks);
            Assert.False(result);
        }

        [Fact]
        public async Task SendInvitations_RepositorySqlException_ReturnsFalse()
        {
            var nicks = new List<string> { "User1" };
            _mockRepo.Setup(r => r.GetPlayerWithDetailsAsync(It.IsAny<string>()))
                     .ThrowsAsync(SqlExceptionBuilder.Build());

            var result = await _helper.SendInvitationsAsync("Code", "Host", nicks);
            Assert.False(result);
        }

        [Fact]
        public async Task SendInvitations_SenderAggregateException_ReturnsFalse()
        {
            var nicks = new List<string> { "User1" };
            SetupMockPlayer("User1", "valid@test.com");

            var aggEx = new AggregateException(new Exception("SMTP 1 Fail"), new Exception("SMTP 2 Fail"));

            _mockSender.Setup(s => s.SendMatchInvitationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                       .ThrowsAsync(aggEx);

            var result = await _helper.SendInvitationsAsync("Code", "Host", nicks);
            Assert.False(result);
        }
    }
}