using System.Linq;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Services.Helpers;
using Xunit;

namespace UnoLisServer.Test
{
    public class ChatSessionHelperTest
    {
        [Fact]
        public void TestChatHistoryCircularBufferKeepsOnlyMaxMessages()
        {
            var helper = ChatSessionHelper.Instance;
            string channel = "TestChannel_Buffer";

            for (int i = 1; i <= 55; i++)
            {
                helper.AddToHistory(channel, new ChatMessageData
                {
                    Nickname = "User",
                    Message = $"Msg {i}"
                });
            }

            var history = helper.GetHistory(channel);

            Assert.Equal(50, history.Count);
            Assert.Equal("Msg 6", history.First().Message);
            Assert.Equal("Msg 55", history.Last().Message);
        }

        [Fact]
        public void TestChatHistoryDifferentChannels_DoNotMix()
        {
            var helper = ChatSessionHelper.Instance;
            helper.AddToHistory("LobbyA", new ChatMessageData { Message = "MessageA" });
            helper.AddToHistory("LobbyB", new ChatMessageData { Message = "MessageB" });

            var historyA = helper.GetHistory("LobbyA");
            var historyB = helper.GetHistory("LobbyB");

            Assert.Single(historyA);
            Assert.Equal("MessageA", historyA[0].Message);
            Assert.Single(historyB);
            Assert.Equal("MessageB", historyB[0].Message);
        }
    }
}