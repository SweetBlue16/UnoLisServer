using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services;
using Xunit;

namespace UnoLisServer.Test.ManagerTest
{
    public class ProfileEditManagerTest
    {
        private readonly Mock<IPlayerRepository> _mockRepository;
        private readonly Mock<IProfileEditCallback> _mockCallback;
        private readonly Mock<INotificationSender> _mockNotificationSender;
        private readonly Mock<IVerificationCodeHelper> _mockVerificationHelper;
        private readonly AutoResetEvent _waitHandle;

        public ProfileEditManagerTest()
        {
            _mockRepository = new Mock<IPlayerRepository>();
            _mockCallback = new Mock<IProfileEditCallback>();
            _mockNotificationSender = new Mock<INotificationSender>();
            _mockVerificationHelper = new Mock<IVerificationCodeHelper>();
            _waitHandle = new AutoResetEvent(false);
        }

        private ProfileEditManager CreateManager()
        {
            return new ProfileEditManager(
                _mockRepository.Object,
                _mockVerificationHelper.Object,
                _mockNotificationSender.Object,
                _mockCallback.Object
            );
        }

        private Player CreateFakePlayer(string nickname, string email = "original@test.com")
        {
            return new Player
            {
                nickname = nickname,
                idPlayer = 1,
                Account = new List<Account> { new Account { email = email, password = PasswordHelper.HashPassword("OldPass1!") } },
                SocialNetwork = new List<SocialNetwork>()
            };
        }

        [Fact]
        public void TestUpdateProfileDataValidDataShouldCallUpdateAndReturnSuccess()
        {
            string nick = "TikiMaster";
            string email = "original@test.com"; 

            var inputData = new ProfileData
            {
                Nickname = nick,
                Email = email,
                FullName = "Tiki Updated",
                FacebookUrl = "http://facebook.com/tiki"
            };

            var existingPlayer = CreateFakePlayer(nick, email);

            _mockRepository.Setup(r => r.GetPlayerProfileByNicknameAsync(nick)).ReturnsAsync(existingPlayer);
            _mockRepository.Setup(r => r.UpdatePlayerProfileAsync(inputData)).Returns(System.Threading.Tasks.Task.CompletedTask);

            _mockCallback.Setup(cb => cb.ProfileUpdateResponse(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.UpdateProfileData(inputData);
            _waitHandle.WaitOne(1000);

            _mockRepository.Verify(r => r.UpdatePlayerProfileAsync(inputData), Times.Once);
            _mockCallback.Verify(cb => cb.ProfileUpdateResponse(
                It.Is<ServiceResponse<ProfileData>>(r => r.Success == true && r.Code == MessageCode.ProfileUpdated)
            ), Times.Once);
        }

        [Fact]
        public void TestUpdateProfileDataDatabaseErrorShouldReturnDatabaseError()
        {
            string nick = "TikiMaster";
            string email = "original@test.com"; 

            var inputData = new ProfileData { Nickname = nick, Email = email };
            var existingPlayer = CreateFakePlayer(nick, email);

            _mockRepository.Setup(r => r.GetPlayerProfileByNicknameAsync(nick)).ReturnsAsync(existingPlayer);

            _mockRepository.Setup(r => r.UpdatePlayerProfileAsync(inputData))
                           .ThrowsAsync(new Exception("DataStore_Unavailable"));

            _mockCallback.Setup(cb => cb.ProfileUpdateResponse(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.UpdateProfileData(inputData);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ProfileUpdateResponse(
                It.Is<ServiceResponse<ProfileData>>(r => r.Success == false && r.Code == MessageCode.DatabaseError)
            ), Times.Once);
        }

        [Fact]
        public void TestUpdateProfileDataSamePasswordShouldReturnSamePasswordError()
        {
            string nick = "TikiMaster";
            string email = "original@test.com";
            string password = "OldPass1!";

            var inputData = new ProfileData
            {
                Nickname = nick,
                Email = email,
                Password = password 
            };

            var existingPlayer = CreateFakePlayer(nick, email);

            _mockRepository.Setup(r => r.GetPlayerProfileByNicknameAsync(nick)).ReturnsAsync(existingPlayer);

            _mockCallback.Setup(cb => cb.ProfileUpdateResponse(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.UpdateProfileData(inputData);
            _waitHandle.WaitOne(1000);

            _mockRepository.Verify(r => r.UpdatePlayerProfileAsync(It.IsAny<ProfileData>()), Times.Never);
            _mockCallback.Verify(cb => cb.ProfileUpdateResponse(
                It.Is<ServiceResponse<ProfileData>>(r => r.Success == false && r.Code == MessageCode.SamePassword)
            ), Times.Once);
        }

        [Fact]
        public void TestUpdateProfileDataPlayerNotFoundShouldReturnNotFoundError()
        {
            var inputData = new ProfileData { Nickname = "GhostUser", Email = "ghost@test.com" };

            _mockRepository.Setup(r => r.GetPlayerProfileByNicknameAsync("GhostUser"))
                           .ReturnsAsync(new Player { idPlayer = 0 });

            _mockCallback.Setup(cb => cb.ProfileUpdateResponse(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.UpdateProfileData(inputData);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ProfileUpdateResponse(
                It.Is<ServiceResponse<ProfileData>>(r => r.Success == false && r.Code == MessageCode.PlayerNotFound)
            ), Times.Once);
        }

        [Fact]
        public void TestUpdateProfileDataWeakPasswordShouldReturnWeakPasswordError()
        {
            var inputData = new ProfileData { Nickname = "Tiki", Email = "a@a.com", Password = "123" };

            _mockCallback.Setup(cb => cb.ProfileUpdateResponse(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.UpdateProfileData(inputData);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ProfileUpdateResponse(
                It.Is<ServiceResponse<ProfileData>>(r => r.Code == MessageCode.WeakPassword)
            ), Times.Once);
        }

        [Fact]
        public void TestUpdateProfileDataInvalidEmailFormatShouldReturnValidationError()
        {
            var inputData = new ProfileData { Nickname = "Tiki", Email = "bad-email" };

            _mockCallback.Setup(cb => cb.ProfileUpdateResponse(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.UpdateProfileData(inputData);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ProfileUpdateResponse(
                It.Is<ServiceResponse<ProfileData>>(r => r.Code == MessageCode.InvalidEmailFormat)
            ), Times.Once);
        }
    }
}