using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services;
using Xunit;

namespace UnoLisServer.Test.ManagerTest
{
    public class AvatarManagerTest
    {
        private readonly Mock<IPlayerRepository> _mockRepository;
        private readonly Mock<IAvatarCallback> _mockCallback;
        private readonly AutoResetEvent _waitHandle;

        public AvatarManagerTest()
        {
            _mockRepository = new Mock<IPlayerRepository>();
            _mockCallback = new Mock<IAvatarCallback>();
            _waitHandle = new AutoResetEvent(false);
        }

        [Fact]
        public void TestGetPlayerAvatarsSuccessReturnsList()
        {
            var list = new List<PlayerAvatar> { new PlayerAvatar { AvatarId = 1 } };
            _mockRepository.Setup(r => r.GetPlayerAvatarsAsync("Tiki")).ReturnsAsync(list);

            _mockCallback.Setup(cb => cb.AvatarsDataReceived(It.IsAny<ServiceResponse<List<PlayerAvatar>>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = new AvatarManager(_mockRepository.Object, _mockCallback.Object);
            manager.GetPlayerAvatars("Tiki");
            _waitHandle.WaitOne(1000);

            _mockCallback.Verify(cb => cb.AvatarsDataReceived(
                It.Is<ServiceResponse<List<PlayerAvatar>>>(r => r.Success && r.Data.Count == 1)
            ));
        }

        [Fact]
        public void TestSetPlayerAvatarValidSelectionUpdatesDB()
        {
            var list = new List<PlayerAvatar> { new PlayerAvatar { AvatarId = 10 } };
            _mockRepository.Setup(r => r.GetPlayerAvatarsAsync("Tiki")).ReturnsAsync(list);

            _mockCallback.Setup(cb => cb.AvatarUpdateResponse(It.IsAny<ServiceResponse<object>>()))
                         .Callback(() => _waitHandle.Set());

            var manager = new AvatarManager(_mockRepository.Object, _mockCallback.Object);

            manager.SetPlayerAvatar("Tiki", 10);
            _waitHandle.WaitOne(1000);

            _mockRepository.Verify(r => r.UpdateSelectedAvatarAsync("Tiki", 10), Times.Once);
            _mockCallback.Verify(cb => cb.AvatarUpdateResponse(
                It.Is<ServiceResponse<object>>(r => r.Success && r.Code == MessageCode.AvatarChanged)
            ));
        }

        [Fact]
        public void TestSetPlayerAvatarLockedSelectionReturnsError()
        {
            var list = new List<PlayerAvatar> { new PlayerAvatar { AvatarId = 10 } };
            _mockRepository.Setup(r => r.GetPlayerAvatarsAsync("Tiki")).ReturnsAsync(list);

            _mockCallback.Setup(cb => cb.AvatarUpdateResponse(It.IsAny<ServiceResponse<object>>())).Callback(() => _waitHandle.Set());

            var manager = new AvatarManager(_mockRepository.Object, _mockCallback.Object);

            manager.SetPlayerAvatar("Tiki", 99);
            _waitHandle.WaitOne(1000);

            _mockRepository.Verify(r => r.UpdateSelectedAvatarAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            _mockCallback.Verify(cb => cb.AvatarUpdateResponse(
                It.Is<ServiceResponse<object>>(r => !r.Success && r.Code == MessageCode.InvalidAvatarSelection)
            ));
        }

    }
}