using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services;
using Xunit;

namespace UnoLisServer.Test.ManagerTest
{
    public class ProfileViewManagerTest
    {
        private readonly Mock<IPlayerRepository> _mockRepository;
        private readonly Mock<IProfileViewCallback> _mockCallback;
        private readonly AutoResetEvent _waitHandle;

        public ProfileViewManagerTest()
        {
            _mockRepository = new Mock<IPlayerRepository>();
            _mockCallback = new Mock<IProfileViewCallback>();
            _waitHandle = new AutoResetEvent(false);
        }

        private ProfileViewManager CreateManager()
        {
            return new ProfileViewManager(_mockRepository.Object, _mockCallback.Object);
        }

        private Player CreateFakePlayer(string nickname)
        {
            return new Player
            {
                idPlayer = 1,
                nickname = nickname,
                fullName = "Test User",
                SelectedAvatar_Avatar_idAvatar = 1,
                Account = new List<Account> { new Account { email = "test@test.com" } },
                PlayerStatistics = new List<PlayerStatistics> { new PlayerStatistics { wins = 5, matchesPlayed = 10, globalPoints = 100 } },
                SocialNetwork = new List<SocialNetwork>(),
                AvatarsUnlocked = new List<AvatarsUnlocked> { new AvatarsUnlocked { Avatar = new Avatar { avatarName = "Avatar1" } } }
            };
        }

        [Fact]
        public void TestGetProfileDataUserExistsShouldReturnSuccessAndData()
        {
            string nickname = "TikiMaster";
            var fakePlayer = CreateFakePlayer(nickname);

            _mockRepository.Setup(r => r.GetPlayerProfileByNicknameAsync(nickname))
                           .ReturnsAsync(fakePlayer);

            _mockCallback.Setup(cb => cb.ProfileDataReceived(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.GetProfileData(nickname);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ProfileDataReceived(
                It.Is<ServiceResponse<ProfileData>>(r => r.Success == true && r.Data.Nickname == nickname && r.Data.Wins == 5)
            ), Times.Once);
        }

        [Fact]
        public void TestGetProfileDataUserNotFoundShouldReturnPlayerNotFoundCode()
        {
            string nickname = "GhostUser";

            _mockRepository.Setup(r => r.GetPlayerProfileByNicknameAsync(nickname))
                           .ReturnsAsync(new Player { idPlayer = 0 });

            _mockCallback.Setup(cb => cb.ProfileDataReceived(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.GetProfileData(nickname);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ProfileDataReceived(
                It.Is<ServiceResponse<ProfileData>>(r => r.Success == false && r.Code == MessageCode.PlayerNotFound)
            ), Times.Once);
        }

        [Fact]
        public void TestGetProfileDataDatabaseErrorShouldReturnDatabaseErrorCode()
        {
            string nickname = "DbErrorUser";

            _mockRepository.Setup(r => r.GetPlayerProfileByNicknameAsync(nickname))
                           .ThrowsAsync(new Exception("DataStore_Unavailable"));

            _mockCallback.Setup(cb => cb.ProfileDataReceived(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.GetProfileData(nickname);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ProfileDataReceived(
                It.Is<ServiceResponse<ProfileData>>(r => r.Success == false && r.Code == MessageCode.DatabaseError)
            ), Times.Once);
        }

        [Fact]
        public void TestGetProfileDataTimeoutShouldReturnTimeoutCode()
        {
            string nickname = "SlowUser";

            _mockRepository.Setup(r => r.GetPlayerProfileByNicknameAsync(nickname))
                           .ThrowsAsync(new Exception("Server_Busy"));

            _mockCallback.Setup(cb => cb.ProfileDataReceived(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.GetProfileData(nickname);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ProfileDataReceived(
                It.Is<ServiceResponse<ProfileData>>(r => r.Success == false && r.Code == MessageCode.Timeout)
            ), Times.Once);
        }

        [Fact]
        public void TestGetProfileDataGeneralExceptionShouldReturnProfileFetchFailed()
        {
            string nickname = "CrashUser";

            _mockRepository.Setup(r => r.GetPlayerProfileByNicknameAsync(nickname))
                           .ThrowsAsync(new Exception("Random Error"));

            _mockCallback.Setup(cb => cb.ProfileDataReceived(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.GetProfileData(nickname);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ProfileDataReceived(
                It.Is<ServiceResponse<ProfileData>>(r => r.Success == false && r.Code == MessageCode.ProfileFetchFailed)
            ), Times.Once);
        }

        [Fact]
        public void TestGetProfileDataGuestUserShouldReturnGuestProfileWithoutRepoCall()
        {
            string nickname = "Guest_123";

            _mockCallback.Setup(cb => cb.ProfileDataReceived(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.GetProfileData(nickname);
            _waitHandle.WaitOne(1000);

            _mockRepository.Verify(r => r.GetPlayerProfileByNicknameAsync(It.IsAny<string>()), Times.Never);

            _mockCallback.Verify(cb => cb.ProfileDataReceived(
                It.Is<ServiceResponse<ProfileData>>(r => r.Success == true && r.Data.FullName == "Guest Player")
            ), Times.Once);
        }
    }
}