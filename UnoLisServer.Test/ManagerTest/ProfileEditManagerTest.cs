using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
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
        private readonly AutoResetEvent _waitHandle;

        public ProfileEditManagerTest()
        {
            _mockRepository = new Mock<IPlayerRepository>();
            _mockCallback = new Mock<IProfileEditCallback>();
            _waitHandle = new AutoResetEvent(false);
        }

        private ProfileEditManager CreateManager()
        {
            return new ProfileEditManager(_mockRepository.Object, _mockCallback.Object);
        }

        [Fact]
        public void UpdateProfileData_ValidData_ShouldCallUpdateAndReturnSuccess()
        {
            var inputData = new ProfileData
            {
                Nickname = "TikiMaster",
                Email = "new@test.com",
                FullName = "Tiki New",
                FacebookUrl = "http://facebook.com/tiki"
            };

            var existingPlayer = CreateFakePlayer("TikiMaster");

            _mockRepository.Setup(r => r.GetPlayerProfileByNicknameAsync("TikiMaster"))
                           .ReturnsAsync(existingPlayer);

            _mockRepository.Setup(r => r.UpdatePlayerProfileAsync(inputData))
                           .Returns(System.Threading.Tasks.Task.CompletedTask);

            _mockCallback.Setup(cb => cb.ProfileUpdateResponse(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.UpdateProfileData(inputData);
            bool signaled = _waitHandle.WaitOne(1000);

            Assert.True(signaled, "Timeout esperando respuesta");

            _mockRepository.Verify(r => r.UpdatePlayerProfileAsync(inputData), Times.Once);

            _mockCallback.Verify(cb => cb.ProfileUpdateResponse(
                It.Is<ServiceResponse<ProfileData>>(r => r.Success == true && r.Code == MessageCode.ProfileUpdated)
            ), Times.Once);
        }

        [Fact]
        public void UpdateProfileData_SamePassword_ShouldReturnSamePasswordError()
        {
            string oldPassword = "OldPassword1!";
            string hashedOld = PasswordHelper.HashPassword(oldPassword);

            var inputData = new ProfileData
            {
                Nickname = "TikiMaster",
                Email = "test@test.com",
                Password = oldPassword
            };

            var existingPlayer = CreateFakePlayer("TikiMaster");
            existingPlayer.Account.First().password = hashedOld;

            _mockRepository.Setup(r => r.GetPlayerProfileByNicknameAsync("TikiMaster"))
                           .ReturnsAsync(existingPlayer);

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
        public void UpdateProfileData_InvalidEmailFormat_ShouldReturnValidationError()
        {
            var inputData = new ProfileData { Nickname = "TikiMaster", Email = "bad-email" };

            _mockCallback.Setup(cb => cb.ProfileUpdateResponse(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            manager.UpdateProfileData(inputData);
            _waitHandle.WaitOne(1000);

            _mockRepository.Verify(r => r.GetPlayerProfileByNicknameAsync(It.IsAny<string>()), Times.Never);

            _mockCallback.Verify(cb => cb.ProfileUpdateResponse(
                It.Is<ServiceResponse<ProfileData>>(r => r.Success == false && r.Code == MessageCode.InvalidEmailFormat)
            ), Times.Once);
        }

        [Fact]
        public void UpdateProfileData_PlayerNotFound_ShouldReturnNotFoundError()
        {
            var inputData = new ProfileData { Nickname = "GhostUser", Email = "ghost@test.com" };

            _mockRepository.Setup(r => r.GetPlayerProfileByNicknameAsync("GhostUser"))
                           .ReturnsAsync((Player)null);

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
        public void UpdateProfileData_DatabaseError_ShouldReturnUpdateFailedError()
        {
            var inputData = new ProfileData { Nickname = "TikiMaster", Email = "ok@test.com" };
            var existingPlayer = CreateFakePlayer("TikiMaster");

            _mockRepository.Setup(r => r.GetPlayerProfileByNicknameAsync("TikiMaster"))
                           .ReturnsAsync(existingPlayer);

            _mockRepository.Setup(r => r.UpdatePlayerProfileAsync(inputData))
                           .ThrowsAsync(new Exception("DB Crash"));

            _mockCallback.Setup(cb => cb.ProfileUpdateResponse(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.UpdateProfileData(inputData);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ProfileUpdateResponse(
                It.Is<ServiceResponse<ProfileData>>(r => r.Success == false && r.Code == MessageCode.ProfileUpdateFailed)
            ), Times.Once);
        }

        [Fact]
        public void UpdateProfileData_WeakPassword_ShouldReturnWeakPasswordError()
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

        private Player CreateFakePlayer(string nickname)
        {
            return new Player
            {
                nickname = nickname,
                idPlayer = 1,
                Account = new List<Account> { new Account { email = "old@test.com", password = "HashedPassword" } },
                SocialNetwork = new List<SocialNetwork>()
            };
        }
    }
}