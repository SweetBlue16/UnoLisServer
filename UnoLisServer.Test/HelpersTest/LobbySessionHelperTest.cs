using Moq;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Contracts.Models;
using UnoLisServer.Services.Helpers;
using Xunit;

namespace UnoLisServer.Test
{
    public class LobbySessionHelperTest
    {
        private readonly LobbySessionHelper _helper;

        public LobbySessionHelperTest()
        {
            _helper = LobbySessionHelper.Instance;
        }

        [Fact]
        public void AddLobby_NewCode_StoresLobby()
        {
            string code = Guid.NewGuid().ToString();
            var lobby = new LobbyInfo(code, new MatchSettings());

            _helper.AddLobby(code, lobby);

            Assert.True(_helper.LobbyExists(code));
            Assert.Same(lobby, _helper.GetLobby(code));
        }

        [Fact]
        public void AddLobby_DuplicateCode_DoesNotOverwriteOrThrow()
        {
            string code = Guid.NewGuid().ToString();
            var lobby1 = new LobbyInfo(code, new MatchSettings());
            var lobby2 = new LobbyInfo(code, new MatchSettings());

            _helper.AddLobby(code, lobby1);
            _helper.AddLobby(code, lobby2); 

            Assert.Same(lobby1, _helper.GetLobby(code));
        }

        [Fact]
        public void RemoveLobby_ExistingCode_RemovesIt()
        {
            string code = Guid.NewGuid().ToString();
            _helper.AddLobby(code, new LobbyInfo(code, new MatchSettings()));

            _helper.RemoveLobby(code);

            Assert.False(_helper.LobbyExists(code));
            Assert.Null(_helper.GetLobby(code));
        }

        [Fact]
        public void RegisterCallback_AddsToList()
        {
            string code = Guid.NewGuid().ToString();
            _helper.AddLobby(code, new LobbyInfo(code, new MatchSettings()));

            var mockCallback = new Mock<ILobbyDuplexCallback>();
            var mockComm = mockCallback.As<ICommunicationObject>();
            mockComm.Setup(c => c.State).Returns(CommunicationState.Opened);

            _helper.RegisterCallback(code, mockCallback.Object);
            _helper.BroadcastToLobby(code, cb => cb.GameStarted());

            mockCallback.Verify(cb => cb.GameStarted(), Times.Once);
        }

        [Fact]
        public void UnregisterCallback_RemovesFromList()
        {
            string code = Guid.NewGuid().ToString();
            _helper.AddLobby(code, new LobbyInfo(code, new MatchSettings()));

            var mockCallback = new Mock<ILobbyDuplexCallback>();
            var mockComm = mockCallback.As<ICommunicationObject>();
            mockComm.Setup(c => c.State).Returns(CommunicationState.Opened);

            _helper.RegisterCallback(code, mockCallback.Object);
            _helper.UnregisterCallback(code, mockCallback.Object);

            _helper.BroadcastToLobby(code, cb => cb.GameStarted());

            mockCallback.Verify(cb => cb.GameStarted(), Times.Never);
        }

        [Fact]
        public void BroadcastToLobby_HandlesExceptionsGracefully()
        {
            string code = Guid.NewGuid().ToString();
            _helper.AddLobby(code, new LobbyInfo(code, new MatchSettings()));

            var failClient = new Mock<ILobbyDuplexCallback>();
            var failComm = failClient.As<ICommunicationObject>();
            failComm.Setup(c => c.State).Returns(CommunicationState.Opened);
            failClient.Setup(c => c.GameStarted()).Throws(new TimeoutException("Lag"));

            var goodClient = new Mock<ILobbyDuplexCallback>();
            var goodComm = goodClient.As<ICommunicationObject>();
            goodComm.Setup(c => c.State).Returns(CommunicationState.Opened);

            _helper.RegisterCallback(code, failClient.Object);
            _helper.RegisterCallback(code, goodClient.Object);

            _helper.BroadcastToLobby(code, cb => cb.GameStarted());

            goodClient.Verify(c => c.GameStarted(), Times.Once);
        }

        [Fact]
        public void ConcurrentAccess_ThreadSafetyTest()
        {
            string code = Guid.NewGuid().ToString();
            _helper.AddLobby(code, new LobbyInfo(code, new MatchSettings()));

            var tasks = new List<Task>();

            for (int i = 0; i < 50; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var mock = new Mock<ILobbyDuplexCallback>();
                    _helper.RegisterCallback(code, mock.Object);
                    _helper.UnregisterCallback(code, mock.Object);
                }));
            }

            var exception = Record.ExceptionAsync(() => Task.WhenAll(tasks));
            Assert.Null(exception.Result);
        }

        [Fact]
        public void BroadcastToLobby_ClientNotOpened_DoesNotInvokeCallback()
        {
            string code = Guid.NewGuid().ToString();
            _helper.AddLobby(code, new LobbyInfo(code, new MatchSettings()));

            var mockCallback = new Mock<ILobbyDuplexCallback>();
            var mockComm = mockCallback.As<ICommunicationObject>();

            mockComm.Setup(c => c.State).Returns(CommunicationState.Closed);

            _helper.RegisterCallback(code, mockCallback.Object);
            _helper.BroadcastToLobby(code, cb => cb.GameStarted());

            mockCallback.Verify(cb => cb.GameStarted(), Times.Never);
        }

        [Fact]
        public void BroadcastToLobby_NoClients_DoesNotThrow()
        {
            string code = Guid.NewGuid().ToString();
            _helper.AddLobby(code, new LobbyInfo(code, new MatchSettings()));

            var exception = Record.Exception(() =>
                _helper.BroadcastToLobby(code, cb => cb.GameStarted()));

            Assert.Null(exception);
        }

        [Fact]
        public void BroadcastToLobby_LobbyDoesNotExist_DoesNotThrow()
        {
            var exception = Record.Exception(() =>
                _helper.BroadcastToLobby("GHOST_CODE", cb => cb.GameStarted()));

            Assert.Null(exception);
        }
    }
}