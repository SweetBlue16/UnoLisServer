using Moq;
using System;
using System.Threading;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services;
using Xunit;

namespace UnoLisServer.Test.ManagerTest
{
    public class RegisterManagerTest
    {
        private readonly Mock<IPlayerRepository> _mockRepository;
        private readonly Mock<IRegisterCallback> _mockCallback;
        private readonly Mock<INotificationSender> _mockNotificationSender;
        private readonly Mock<IVerificationCodeHelper> _mockVerificationHelper;
        private readonly Mock<IPendingRegistrationHelper> _mockPendingHelper;

        private readonly AutoResetEvent _waitHandle;

        public RegisterManagerTest()
        {
            _mockRepository = new Mock<IPlayerRepository>();
            _mockCallback = new Mock<IRegisterCallback>();
            _mockNotificationSender = new Mock<INotificationSender>();
            _mockVerificationHelper = new Mock<IVerificationCodeHelper>();
            _mockPendingHelper = new Mock<IPendingRegistrationHelper>();

            _waitHandle = new AutoResetEvent(false);
        }

        private RegisterManager CreateManager()
        {
            return new RegisterManager(
                _mockRepository.Object,
                _mockCallback.Object,
                _mockNotificationSender.Object,
                _mockVerificationHelper.Object,
                _mockPendingHelper.Object
            );
        }

        [Fact]
        public void Register_ValidData_ShouldProcessSuccessfully()
        {
            var data = new RegistrationData
            {
                Nickname = "NewUser",
                Email = "new@test.com",
                Password = "Pass",
                FullName = "Name"
            };

            _mockRepository.Setup(r => r.IsNicknameTakenAsync(data.Nickname)).ReturnsAsync(false);
            _mockRepository.Setup(r => r.IsEmailRegisteredAsync(data.Email)).ReturnsAsync(false);
            _mockVerificationHelper.Setup(v => v.CanRequestCode(data.Email, CodeType.EmailVerification)).Returns(true);
            _mockVerificationHelper.Setup(v => v.GenerateAndStoreCode(data.Email, CodeType.EmailVerification)).Returns("123456");

            _mockCallback.Setup(cb => cb.RegisterResponse(It.IsAny<ServiceResponse<object>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.Register(data);
            _waitHandle.WaitOne(1000);

            _mockPendingHelper.Verify(p => p.StorePendingRegistration(data.Email, It.IsAny<PendingRegistration>()), Times.Once);

            _mockNotificationSender.Verify(n => n.SendAccountVerificationEmailAsync(data.Email, "123456"), Times.Once);

            _mockCallback.Verify(cb => cb.RegisterResponse(
                It.Is<ServiceResponse<object>>(r => r.Success == true && r.Code == MessageCode.VerificationCodeSent)
            ), Times.Once);
        }

        [Fact]
        public void Register_NicknameTaken_ShouldReturnError()
        {
            var data = new RegistrationData { Nickname = "TakenNick", Email = "a@a.com", Password = "P", FullName = "F" };

            _mockRepository.Setup(r => r.IsNicknameTakenAsync("TakenNick")).ReturnsAsync(true); // ¡Ya existe!

            _mockCallback.Setup(cb => cb.RegisterResponse(It.IsAny<ServiceResponse<object>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.Register(data);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.RegisterResponse(
                It.Is<ServiceResponse<object>>(r => r.Success == false && r.Code == MessageCode.NicknameAlreadyTaken)
            ), Times.Once);

            _mockNotificationSender.Verify(n => n.SendAccountVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Register_EmailRegistered_ShouldReturnError()
        {
            var data = new RegistrationData { Nickname = "FreeNick", Email = "taken@test.com", Password = "P", FullName = "F" };

            _mockRepository.Setup(r => r.IsNicknameTakenAsync("FreeNick")).ReturnsAsync(false);
            _mockRepository.Setup(r => r.IsEmailRegisteredAsync("taken@test.com")).ReturnsAsync(true); 

            _mockCallback.Setup(cb => cb.RegisterResponse(It.IsAny<ServiceResponse<object>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.Register(data);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.RegisterResponse(
                It.Is<ServiceResponse<object>>(r => r.Success == false && r.Code == MessageCode.EmailAlreadyRegistered)
            ), Times.Once);
        }

        [Fact]
        public void Register_RateLimitExceeded_ShouldReturnError()
        {
            var data = new RegistrationData { Nickname = "Spammer", Email = "spam@test.com", Password = "P", FullName = "F" };

            _mockRepository.Setup(r => r.IsNicknameTakenAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockRepository.Setup(r => r.IsEmailRegisteredAsync(It.IsAny<string>())).ReturnsAsync(false);

            _mockVerificationHelper.Setup(v => v.CanRequestCode(data.Email, CodeType.EmailVerification)).Returns(false);

            _mockCallback.Setup(cb => cb.RegisterResponse(It.IsAny<ServiceResponse<object>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.Register(data);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.RegisterResponse(
                It.Is<ServiceResponse<object>>(r => r.Success == false && r.Code == MessageCode.RateLimitExceeded)
            ), Times.Once);
        }

        [Fact]
        public void Register_InvalidFormat_ShouldReturnValidationError()
        {
            var data = new RegistrationData { Nickname = "User", Email = "", Password = "P", FullName = "F" };

            _mockCallback.Setup(cb => cb.RegisterResponse(It.IsAny<ServiceResponse<object>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.Register(data);
            _waitHandle.WaitOne(1000);

            _mockRepository.Verify(r => r.IsNicknameTakenAsync(It.IsAny<string>()), Times.Never);

            _mockCallback.Verify(cb => cb.RegisterResponse(
                It.Is<ServiceResponse<object>>(r => r.Success == false && r.Code == MessageCode.EmptyFields)
            ), Times.Once);
        }

        [Fact]
        public void Register_DatabaseError_ShouldReturnInternalError()
        {
            var data = new RegistrationData { Nickname = "User", Email = "a@a.com", Password = "P", FullName = "F" };

            _mockRepository.Setup(r => r.IsNicknameTakenAsync(It.IsAny<string>()))
                           .ThrowsAsync(new Exception("DB Connection Failed"));

            _mockCallback.Setup(cb => cb.RegisterResponse(It.IsAny<ServiceResponse<object>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.Register(data);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.RegisterResponse(
                It.Is<ServiceResponse<object>>(r => r.Success == false && r.Code == MessageCode.RegistrationInternalError)
            ), Times.Once);
        }

        [Fact]
        public void Register_NullData_ShouldReturnValidationError()
        {
            _mockCallback.Setup(cb => cb.RegisterResponse(It.IsAny<ServiceResponse<object>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.Register(null);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.RegisterResponse(
                It.Is<ServiceResponse<object>>(r => r.Success == false && r.Code == MessageCode.EmptyFields)
            ), Times.Once);
        }
        [Fact]
        public void Register_EmailSendingFails_ShouldReturnInternalError()
        {
            var data = new RegistrationData { Nickname = "User", Email = "a@a.com", Password = "P", FullName = "F" };

            _mockRepository.Setup(r => r.IsNicknameTakenAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockRepository.Setup(r => r.IsEmailRegisteredAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockVerificationHelper.Setup(v => v.CanRequestCode(It.IsAny<string>(), CodeType.EmailVerification)).Returns(true);
            _mockVerificationHelper.Setup(v => v.GenerateAndStoreCode(It.IsAny<string>(), CodeType.EmailVerification)).Returns("123456");

            _mockNotificationSender.Setup(n => n.SendAccountVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                                   .ThrowsAsync(new Exception("SMTP Server Timeout"));

            _mockCallback.Setup(cb => cb.RegisterResponse(It.IsAny<ServiceResponse<object>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.Register(data);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.RegisterResponse(
                It.Is<ServiceResponse<object>>(r => r.Success == false && r.Code == MessageCode.RegistrationInternalError)
            ), Times.Once);
        }

        [Fact]
        public void Register_PendingStorageFails_ShouldReturnInternalError()
        {
            var data = new RegistrationData { Nickname = "User", Email = "a@a.com", Password = "P", FullName = "F" };

            _mockRepository.Setup(r => r.IsNicknameTakenAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockRepository.Setup(r => r.IsEmailRegisteredAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockVerificationHelper.Setup(v => v.CanRequestCode(It.IsAny<string>(), CodeType.EmailVerification)).Returns(true);

            _mockPendingHelper.Setup(p => p.StorePendingRegistration(It.IsAny<string>(), It.IsAny<PendingRegistration>()))
                              .Throws(new Exception("Memory Error"));

            _mockCallback.Setup(cb => cb.RegisterResponse(It.IsAny<ServiceResponse<object>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.Register(data);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.RegisterResponse(
                It.Is<ServiceResponse<object>>(r => r.Success == false && r.Code == MessageCode.RegistrationInternalError)
            ), Times.Once);
        }

        [Fact]
        public void Register_CodeGenerationFails_ShouldReturnInternalError()
        {
            var data = new RegistrationData { Nickname = "User", Email = "a@a.com", Password = "P", FullName = "F" };

            _mockRepository.Setup(r => r.IsNicknameTakenAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockVerificationHelper.Setup(v => v.CanRequestCode(It.IsAny<string>(), CodeType.EmailVerification)).Returns(true);

            _mockVerificationHelper.Setup(v => v.GenerateAndStoreCode(It.IsAny<string>(), CodeType.EmailVerification))
                                   .Throws(new Exception("Random Generator Failed"));

            _mockCallback.Setup(cb => cb.RegisterResponse(It.IsAny<ServiceResponse<object>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.Register(data);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.RegisterResponse(
                It.Is<ServiceResponse<object>>(r => r.Success == false && r.Code == MessageCode.RegistrationInternalError)
            ), Times.Once);
        }
    }
}