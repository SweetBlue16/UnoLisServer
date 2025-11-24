using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
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

        [Fact]
        public void GetProfileData_UserExists_ShouldReturnSuccessAndData()
        {
            string nickname = "TikiMaster";
            var fakePlayer = CreateFakePlayer(nickname);

            _mockRepository.Setup(repo => repo.GetPlayerProfileByNicknameAsync(nickname))
                           .ReturnsAsync(fakePlayer);

            _mockCallback.Setup(cb => cb.ProfileDataReceived(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.GetProfileData(nickname);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ProfileDataReceived(
                It.Is<ServiceResponse<ProfileData>>(r => r.Success == true && r.Data.Nickname == "TikiMaster")
            ), Times.Once);
        }

        [Fact]
        public void GetProfileData_UserNotFound_ShouldReturnPlayerNotFoundCode()
        {
            string nickname = "GhostUser";
            _mockRepository.Setup(repo => repo.GetPlayerProfileByNicknameAsync(nickname))
                           .ReturnsAsync((Player)null);

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
        public void GetProfileData_DatabaseError_ShouldReturnDatabaseErrorCode()
        {
            string nickname = "ErrorUser";
            var sqlException = FormatterServices.GetUninitializedObject(typeof(SqlException)) as SqlException;

            _mockRepository.Setup(repo => repo.GetPlayerProfileByNicknameAsync(nickname))
                           .ThrowsAsync(sqlException);

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
        public void GetProfileData_TimeoutError_ShouldReturnTimeoutCode()
        {
            string nickname = "SlowUser";
            _mockRepository.Setup(repo => repo.GetPlayerProfileByNicknameAsync(nickname))
                           .ThrowsAsync(new TimeoutException("DB Timeout"));

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
        public void GetProfileData_UserHasNoSelectedAvatar_ShouldReturnDefaultLogoUNO()
        {
            string nickname = "NoAvatarUser";
            var fakePlayer = CreateFakePlayer(nickname);
            fakePlayer.SelectedAvatar_Avatar_idAvatar = null;

            _mockRepository.Setup(repo => repo.GetPlayerProfileByNicknameAsync(nickname))
                           .ReturnsAsync(fakePlayer);

            _mockCallback.Setup(cb => cb.ProfileDataReceived(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.GetProfileData(nickname);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ProfileDataReceived(
                It.Is<ServiceResponse<ProfileData>>(r => r.Data.SelectedAvatarName == "LogoUNO")
            ), Times.Once);
        }

        [Fact]
        public void GetProfileData_UserHasNoStats_ShouldReturnZeroes()
        {
            string nickname = "NewUser";
            var fakePlayer = new Player { nickname = nickname, Account = new List<Account>(), PlayerStatistics = new List<PlayerStatistics>(), SocialNetwork = new List<SocialNetwork>(), AvatarsUnlocked = new List<AvatarsUnlocked>() };

            _mockRepository.Setup(repo => repo.GetPlayerProfileByNicknameAsync(nickname))
                           .ReturnsAsync(fakePlayer);

            _mockCallback.Setup(cb => cb.ProfileDataReceived(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.GetProfileData(nickname);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ProfileDataReceived(
                It.Is<ServiceResponse<ProfileData>>(r =>
                    r.Success == true &&
                    r.Data.Wins == 0 &&
                    r.Data.MatchesPlayed == 0)
            ), Times.Once);
        }

        [Fact]
        public void GetProfileData_GeneralException_ShouldReturnFetchFailedCode()
        {
            string nickname = "CrashUser";
            _mockRepository.Setup(repo => repo.GetPlayerProfileByNicknameAsync(nickname))
                           .ThrowsAsync(new Exception("Unknown error"));

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
        public void GetProfileData_NullNickname_ShouldHandleGracefully()
        {
            _mockRepository.Setup(repo => repo.GetPlayerProfileByNicknameAsync(It.IsAny<string>()))
                           .ReturnsAsync((Player)null);

            _mockCallback.Setup(cb => cb.ProfileDataReceived(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();
            manager.GetProfileData(null);
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.ProfileDataReceived(
                It.Is<ServiceResponse<ProfileData>>(r => r.Success == false)
            ), Times.Once);
        }

        private Player CreateFakePlayer(string nickname)
        {
            return new Player
            {
                nickname = nickname,
                fullName = "Test User",
                SelectedAvatar_Avatar_idAvatar = 1,
                Account = new List<Account> { new Account { email = "test@test.com" } },
                PlayerStatistics = new List<PlayerStatistics> { new PlayerStatistics { wins = 10, globalPoints = 100 } },
                SocialNetwork = new List<SocialNetwork>(),
                AvatarsUnlocked = new List<AvatarsUnlocked> { new AvatarsUnlocked { Avatar = new Avatar { avatarName = "CoolAvatar" } } }
            };
        }
    }
}