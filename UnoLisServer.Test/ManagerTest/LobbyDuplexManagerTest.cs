using Moq;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Services;
using UnoLisServer.Services.ManagerInterfaces;
using Xunit;

namespace UnoLisServer.Test
{
    public class LobbyDuplexManagerTest
    {
        private readonly Mock<ILobbyManager> _mockLobbyManager;
        private readonly LobbyDuplexManager _duplexManager;

        public LobbyDuplexManagerTest()
        {
            _mockLobbyManager = new Mock<ILobbyManager>();
            _duplexManager = new LobbyDuplexManager(_mockLobbyManager.Object);
        }

        [Fact]
        public void TestConnectToLobbyDelegatesToManager()
        {
            string code = "CODE1";
            string nick = "User1";

            _duplexManager.ConnectToLobby(code, nick);
            _mockLobbyManager.Verify(x => x.RegisterConnection(code, nick), Times.Once);
        }

        [Fact]
        public void TestDisconnectFromLobbyDelegatesToManagerAndClearsState()
        {
            string code = "CODE1";
            string nick = "User1";
            _duplexManager.ConnectToLobby(code, nick); 
            _duplexManager.DisconnectFromLobby(code, nick);

            _mockLobbyManager.Verify(x => x.RemoveConnection(code, nick, It.IsAny<ILobbyDuplexCallback>()), Times.Once);
            _duplexManager.Dispose();
            _mockLobbyManager.Verify(x => x.RemoveConnection(code, nick, It.IsAny<ILobbyDuplexCallback>()), Times.Once); 
        }

        [Fact]
        public void TestDisposeAbruptDisconnectionTriggersCleanup()
        {
            string code = "CODE_ABRUPT";
            string nick = "UserGhost";

            _duplexManager.ConnectToLobby(code, nick);
            _duplexManager.Dispose(); 

            _mockLobbyManager.Verify(x => x.RemoveConnection(code, nick, It.IsAny<ILobbyDuplexCallback>()), Times.Once);
        }

        [Fact]
        public void TestDisposeCleanDisconnectionDoesNotTriggerDoubleCleanup()
        {
            string code = "CODE_CLEAN";
            string nick = "GoodUser";

            _duplexManager.ConnectToLobby(code, nick);
            _duplexManager.DisconnectFromLobby(code, nick);
            _duplexManager.Dispose();
            _mockLobbyManager.Verify(x => x.RemoveConnection(code, nick, It.IsAny<ILobbyDuplexCallback>()), Times.Once);
        }

        [Fact]
        public async Task TestSetReadyStatusFireAndForgetCallsManager()
        {
            string code = "C";
            string nick = "U";
            bool status = true;

            _duplexManager.SetReadyStatus(code, nick, status);
            await Task.Delay(100);

            _mockLobbyManager.Verify(x => x.HandleReadyStatusAsync(code, nick, status), Times.Once);
        }

        [Fact]
        public void TestCleanUpHandlesExceptionDoesNotCrash()
        {
            string code = "ERR";
            string nick = "User";
            _duplexManager.ConnectToLobby(code, nick);

            _mockLobbyManager.Setup(x => x.RemoveConnection(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ILobbyDuplexCallback>()))
                             .Throws(new Exception("Manager Crash"));

            var ex = Record.Exception(() => _duplexManager.DisconnectFromLobby(code, nick));

            Assert.Null(ex);
            _mockLobbyManager.Verify(x => x.RemoveConnection(code, nick, It.IsAny<ILobbyDuplexCallback>()), Times.Once);
        }
    }
}