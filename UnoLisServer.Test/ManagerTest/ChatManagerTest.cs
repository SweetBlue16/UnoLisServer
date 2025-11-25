using Moq;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Services;
using UnoLisServer.Services.Helpers;
using UnoLisServer.Services.Providers;
using Xunit;

namespace UnoLisServer.Test
{
    public class ChatManagerTest
    {
        private readonly Mock<IChatSessionHelper> _mockSessionHelper;
        private readonly Mock<IChatCallbackProvider> _mockCallbackProvider;
        private readonly Mock<IChatCallback> _mockCallback;
        private readonly ChatManager _manager;

        public ChatManagerTest()
        {
            _mockSessionHelper = new Mock<IChatSessionHelper>();
            _mockCallbackProvider = new Mock<IChatCallbackProvider>();
            _mockCallback = new Mock<IChatCallback>();

            _mockCallbackProvider.Setup(x => x.GetCallback()).Returns(_mockCallback.Object);

            _manager = new ChatManager(_mockSessionHelper.Object, _mockCallbackProvider.Object);
        }

        [Fact]
        public void TestRegisterPlayer_ValidNickname_AddsClientToSession()
        {
            string nickname = "TikiTest";

            _manager.RegisterPlayer(nickname);
            _mockSessionHelper.Verify(x => x.AddClient(nickname, _mockCallback.Object), Times.Once);
        }

        [Fact]
        public void TestRegisterPlayer_NullNickname_DoesNotAddClient()
        {
            string nickname = null;

            _manager.RegisterPlayer(nickname);
            _mockSessionHelper.Verify(x => x.AddClient(It.IsAny<string>(), It.IsAny<IChatCallback>()), Times.Never);
        }

        [Fact]
        public void TestSendMessage_ValidMessage_ValidatesSavesAndBroadcasts()
        {
            var msg = new ChatMessageData { Nickname = "User", Message = "Hello World", ChannelId = "Lobby1" };

            var client1 = new Mock<IChatCallback>();
            var client2 = new Mock<IChatCallback>();
            var activeClients = new List<IChatCallback> { client1.Object, client2.Object };

            _mockSessionHelper.Setup(x => x.GetActiveClients()).Returns(activeClients);
            _manager.SendMessage(msg);

            _mockSessionHelper.Verify(x => x.AddToHistory("Lobby1", msg), Times.Once);

            client1.Verify(x => x.MessageReceived(msg), Times.Once);
            client2.Verify(x => x.MessageReceived(msg), Times.Once);
        }

        [Fact]
        public void TestSendMessage_EmptyMessage_StopsAtValidation()
        {
            var msg = new ChatMessageData { Nickname = "User", Message = "" }; 

            _manager.SendMessage(msg);

            _mockSessionHelper.Verify(x => x.AddToHistory(It.IsAny<string>(), It.IsAny<ChatMessageData>()), Times.Never);
            _mockSessionHelper.Verify(x => x.GetActiveClients(), Times.Never);
        }

        [Fact]
        public void TestSendMessage_WhenOneClientFails_OtherClientsStillReceiveMessage()
        {
            var msg = new ChatMessageData { Nickname = "User", Message = "Hi" };

            var clientOk = new Mock<IChatCallback>();
            var clientFail = new Mock<IChatCallback>();

            clientFail.Setup(x => x.MessageReceived(It.IsAny<ChatMessageData>()))
                      .Throws(new CommunicationException("Disconnected"));

            var activeClients = new List<IChatCallback> { clientFail.Object, clientOk.Object };
            _mockSessionHelper.Setup(x => x.GetActiveClients()).Returns(activeClients);
            _manager.SendMessage(msg);


            clientOk.Verify(x => x.MessageReceived(msg), Times.Once);
        }

        [Fact]
        public void TestGetChatHistory_RetrievesAndSendsToRequester()
        {
            string channel = "LobbyA";
            var fakeHistory = new List<ChatMessageData>
            {
                new ChatMessageData { Message = "Msg1" }
            };

            _mockSessionHelper.Setup(x => x.GetHistory(channel)).Returns(fakeHistory);
            _manager.GetChatHistory(channel);
            _mockCallback.Verify(x => x.ChatHistoryReceived(It.Is<ChatMessageData[]>(arr => arr.Length == 1)), Times.Once);
        }
    }
}