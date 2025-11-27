using UnoLisServer.Common.Exceptions;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Services.Validators;
using Xunit;

namespace UnoLisServer.Test
{
    public class ChatValidatorTest
    {
        [Fact]
        public void TestValidateMessageValidDataDoesNotThrow()
        {
            var msg = new ChatMessageData { Nickname = "A", Message = "Hola" };
            ChatValidator.ValidateMessage(msg); 
        }

        [Fact]
        public void TestValidateMessageNullObjectThrowsValidationException()
        {
            ChatMessageData msg = null;
            Assert.Throws<ValidationException>(() => ChatValidator.ValidateMessage(msg));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateMessageInvalidNicknameThrowsValidationException(string badNick)
        {
            var msg = new ChatMessageData { Nickname = badNick, Message = "Hola" };
            Assert.Throws<ValidationException>(() => ChatValidator.ValidateMessage(msg));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TestValidateMessageInvalidMessageContentThrowsValidationException(string badMsg)
        {
            var msg = new ChatMessageData { Nickname = "User", Message = badMsg };
            Assert.Throws<ValidationException>(() => ChatValidator.ValidateMessage(msg));
        }

        [Fact]
        public void TestValidateMessageMessageTooLongThrowsValidationException()
        {
            string longMsg = new string('A', 256);
            var msg = new ChatMessageData { Nickname = "User", Message = longMsg };

            var ex = Assert.Throws<ValidationException>(() => ChatValidator.ValidateMessage(msg));
            Assert.Contains("255", ex.Message); 
        }
    }
}