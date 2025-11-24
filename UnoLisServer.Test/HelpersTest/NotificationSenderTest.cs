using Moq;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using Xunit;

namespace UnoLisServer.Test.HelpersTest
{
    public class NotificationSenderTest
    {
        private readonly Mock<IEmailSender> _mockEmailSender;
        private readonly NotificationSender _notificationSender;

        public NotificationSenderTest()
        {
            _mockEmailSender = new Mock<IEmailSender>();
            _notificationSender = new NotificationSender(_mockEmailSender.Object);
        }

        [Fact]
        public async Task TestSendAccountVerificationEmailShouldContainCodeAndCorrectSubject()
        {
            string email = "user@test.com";
            string code = "123456";

            await _notificationSender.SendAccountVerificationEmailAsync(email, code);

            _mockEmailSender.Verify(x => x.SendEmailAsync(
                email,
                "Código de Verificación - UNO LIS",
                It.Is<string>(body => body.Contains(code) && body.Contains("Gracias por registrarte"))
            ), Times.Once);
        }

        [Fact]
        public async Task TestSendMatchInvitationAsyncShouldContainLobbyCode()
        {
            string email = "friend@test.com";
            string inviter = "TikiMaster";
            string lobbyCode = "LOBBY-99";

            await _notificationSender.SendMatchInvitationAsync(email, inviter, lobbyCode);

            _mockEmailSender.Verify(x => x.SendEmailAsync(
                email,
                It.IsAny<string>(),
                It.Is<string>(body =>
                    body.Contains(inviter) &&
                    body.Contains(lobbyCode) &&
                    body.Contains("Join Match"))
            ), Times.Once);
        }
    }
}