using Moq;
using System;
using System.Threading;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services;
using Xunit;

namespace UnoLisServer.Test.ManagerTest
{
    public class ConfirmationManagerTest
    {
        private readonly Mock<IPlayerRepository> _mockRepository;
        private readonly Mock<IConfirmationCallback> _mockCallback;
        private readonly Mock<INotificationSender> _mockNotificationSender;
        private readonly Mock<IVerificationCodeHelper> _mockVerificationHelper;
        private readonly Mock<IPendingRegistrationHelper> _mockPendingHelper;
        private readonly AutoResetEvent _waitHandle;

        public ConfirmationManagerTest()
        {
            _mockRepository = new Mock<IPlayerRepository>();
            _mockCallback = new Mock<IConfirmationCallback>();
            _mockNotificationSender = new Mock<INotificationSender>();
            _mockVerificationHelper = new Mock<IVerificationCodeHelper>();
            _mockPendingHelper = new Mock<IPendingRegistrationHelper>();
            _waitHandle = new AutoResetEvent(false);
        }

        private ConfirmationManager CreateManager()
        {
            return new ConfirmationManager(
                _mockRepository.Object,
                _mockCallback.Object,
                _mockNotificationSender.Object,
                _mockVerificationHelper.Object,
                _mockPendingHelper.Object
            );
        }

        [Fact]
        public void TestConfirmCodeWithValidDataShouldCreateAccount()
        {
            string email = "confirm@test.com";
            string code = "123456";
            var pending = new PendingRegistration { Nickname = "PendingNick" };

            _mockVerificationHelper.Setup(v => v.ValidateCode(It.IsAny<CodeValidationRequest>())).Returns(true);
            _mockPendingHelper.Setup(p => p.GetAndRemovePendingRegistration(email)).Returns(pending);
            _mockRepository.Setup(r => r.CreatePlayerFromPendingAsync(email, pending)).Returns(System.Threading.Tasks.Task.CompletedTask);

            _mockCallback.Setup(cb => cb.ConfirmationResponse(It.IsAny<ServiceResponse<object>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.ConfirmCode(email, code);
            _waitHandle.WaitOne(1000);

            _mockRepository.Verify(r => r.CreatePlayerFromPendingAsync(email, pending), Times.Once);
            _mockCallback.Verify(cb => cb.ConfirmationResponse(
                It.Is<ServiceResponse<object>>(r => r.Success == true && r.Code == MessageCode.RegistrationSuccessful)
            ), Times.Once);
        }

        [Fact]
        public void TestConfirmCodeWithInvalidCodeShouldReturnError()
        {
            _mockVerificationHelper.Setup(v => v.ValidateCode(It.IsAny<CodeValidationRequest>())).Returns(false);
            _mockCallback.Setup(cb => cb.ConfirmationResponse(It.IsAny<ServiceResponse<object>>())).Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.ConfirmCode("a@a.com", "WrongCode");
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ConfirmationResponse(
                It.Is<ServiceResponse<object>>(r => r.Success == false && r.Code == MessageCode.VerificationCodeInvalid)
            ), Times.Once);
        }

        [Fact]
        public void TestConfirmCodeWithMissingPendingDataShouldReturnError()
        {
            _mockVerificationHelper.Setup(v => v.ValidateCode(It.IsAny<CodeValidationRequest>())).Returns(true);
            _mockPendingHelper.Setup(p => p.GetAndRemovePendingRegistration(It.IsAny<string>())).Returns((PendingRegistration)null);

            _mockCallback.Setup(cb => cb.ConfirmationResponse(It.IsAny<ServiceResponse<object>>())).Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.ConfirmCode("a@a.com", "123456");
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ConfirmationResponse(
                It.Is<ServiceResponse<object>>(r => r.Success == false && r.Code == MessageCode.RegistrationDataLost)
            ), Times.Once);
        }

        [Fact]
        public void TestConfirmCodeWithDatabaseErrorShouldReturnInternalError()
        {
            var pending = new PendingRegistration();
            _mockVerificationHelper.Setup(v => v.ValidateCode(It.IsAny<CodeValidationRequest>())).Returns(true);
            _mockPendingHelper.Setup(p => p.GetAndRemovePendingRegistration(It.IsAny<string>())).Returns(pending);

            _mockRepository.Setup(r => r.CreatePlayerFromPendingAsync(It.IsAny<string>(), pending))
                           .ThrowsAsync(new Exception("DB Fail"));

            _mockCallback.Setup(cb => cb.ConfirmationResponse(It.IsAny<ServiceResponse<object>>())).Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.ConfirmCode("a@a.com", "123456");
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ConfirmationResponse(
                It.Is<ServiceResponse<object>>(r => r.Code == MessageCode.ConfirmationInternalError)
            ), Times.Once);
        }

        [Fact]
        public void TestResendConfirmationCodeWithValidEmailShouldSendEmail()
        {
            string email = "resend@test.com";
            _mockVerificationHelper.Setup(v => v.CanRequestCode(email, CodeType.EmailVerification)).Returns(true);
            _mockVerificationHelper.Setup(v => v.GenerateAndStoreCode(email, CodeType.EmailVerification)).Returns("654321");

            _mockCallback.Setup(cb => cb.ResendCodeResponse(It.IsAny<ServiceResponse<object>>())).Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.ResendConfirmationCode(email);
            _waitHandle.WaitOne(1000);

            _mockNotificationSender.Verify(n => n.SendAccountVerificationEmailAsync(email, "654321"), Times.Once);
            _mockCallback.Verify(cb => cb.ResendCodeResponse(
                It.Is<ServiceResponse<object>>(r => r.Success == true && r.Code == MessageCode.VerificationCodeResent)
            ), Times.Once);
        }

        [Fact]
        public void TestResendConfirmationCodeRateLimitedShouldReturnError()
        {
            _mockVerificationHelper.Setup(v => v.CanRequestCode(It.IsAny<string>(), CodeType.EmailVerification)).Returns(false);
            _mockCallback.Setup(cb => cb.ResendCodeResponse(It.IsAny<ServiceResponse<object>>())).Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.ResendConfirmationCode("spam@test.com");
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ResendCodeResponse(
                It.Is<ServiceResponse<object>>(r => r.Success == false && r.Code == MessageCode.RateLimitExceeded)
            ), Times.Once);
        }
    }
}