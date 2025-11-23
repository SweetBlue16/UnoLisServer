using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Threading; // Necesario para AutoResetEvent
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
            // ARRANGE
            string nickname = "TikiMaster";
            var fakePlayer = new Player
            {
                nickname = nickname,
                fullName = "Tiki",
                Account = new List<Account> { new Account { email = "tiki@test.com" } },
                PlayerStatistics = new List<PlayerStatistics> { new PlayerStatistics { wins = 10, globalPoints = 100 } },
                SocialNetwork = new List<SocialNetwork>(),
                AvatarsUnlocked = new List<AvatarsUnlocked>()
            };

            _mockRepository.Setup(repo => repo.GetPlayerProfileByNicknameAsync(nickname))
                           .ReturnsAsync(fakePlayer);

            // CONFIGURAMOS EL CALLBACK PARA QUE LIBERE EL SEMÁFORO
            _mockCallback.Setup(cb => cb.ProfileDataReceived(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            // ACT
            manager.GetProfileData(nickname);

            // Esperamos hasta 1 segundo a que el callback se ejecute.
            // Si devuelve false, es que hubo timeout (el código falló o se colgó).
            bool signaled = _waitHandle.WaitOne(2000);

            // ASSERT
            Assert.True(signaled, "El callback no fue invocado a tiempo (Timeout).");

            _mockCallback.Verify(cb => cb.ProfileDataReceived(
                It.Is<ServiceResponse<ProfileData>>(r =>
                    r.Success == true &&
                    r.Data.Nickname == "TikiMaster")
            ), Times.Once);
        }

        [Fact]
        public void GetProfileData_UserNotFound_ShouldReturnPlayerNotFoundCode()
        {
            // ARRANGE
            string nickname = "GhostUser";

            _mockRepository.Setup(repo => repo.GetPlayerProfileByNicknameAsync(nickname))
                           .ReturnsAsync((Player)null);

            // Configurar espera
            _mockCallback.Setup(cb => cb.ProfileDataReceived(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            // ACT
            manager.GetProfileData(nickname);
            bool signaled = _waitHandle.WaitOne(2000);

            // ASSERT
            Assert.True(signaled, "Timeout esperando respuesta.");
            _mockCallback.Verify(cb => cb.ProfileDataReceived(
                It.Is<ServiceResponse<ProfileData>>(r =>
                    r.Success == false &&
                    r.Code == MessageCode.PlayerNotFound)
            ), Times.Once);
        }

        [Fact]
        public void GetProfileData_DatabaseError_ShouldReturnDatabaseErrorCode()
        {
            // ARRANGE
            string nickname = "ErrorUser";
            var sqlException = FormatterServices.GetUninitializedObject(typeof(SqlException)) as SqlException;

            _mockRepository.Setup(repo => repo.GetPlayerProfileByNicknameAsync(nickname))
                           .ThrowsAsync(sqlException);

            _mockCallback.Setup(cb => cb.ProfileDataReceived(It.IsAny<ServiceResponse<ProfileData>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = CreateManager();

            // ACT
            manager.GetProfileData(nickname);
            bool signaled = _waitHandle.WaitOne(2000);

            // ASSERT
            Assert.True(signaled, "Timeout esperando reporte de error.");
            _mockCallback.Verify(cb => cb.ProfileDataReceived(
                It.Is<ServiceResponse<ProfileData>>(r =>
                    r.Success == false &&
                    r.Code == MessageCode.DatabaseError)
            ), Times.Once);
        }
    }
}