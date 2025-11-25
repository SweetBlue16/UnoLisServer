using UnoLisServer.Common.Exceptions;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Services.Validators;
using Xunit;

namespace UnoLisServer.Test
{
    public class ChatValidatorTest
    {
        [Fact]
        public void ValidateMessage_ValidData_DoesNotThrow()
        {
            var msg = new ChatMessageData { Nickname = "A", Message = "Hola" };
            ChatValidator.ValidateMessage(msg); 
        }

        [Fact]
        public void ValidateMessage_NullObject_ThrowsValidationException()
        {
            ChatMessageData msg = null;
            Assert.Throws<ValidationException>(() => ChatValidator.ValidateMessage(msg));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateMessage_InvalidNickname_ThrowsValidationException(string badNick)
        {
            var msg = new ChatMessageData { Nickname = badNick, Message = "Hola" };
            Assert.Throws<ValidationException>(() => ChatValidator.ValidateMessage(msg));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateMessage_InvalidMessageContent_ThrowsValidationException(string badMsg)
        {
            var msg = new ChatMessageData { Nickname = "User", Message = badMsg };
            Assert.Throws<ValidationException>(() => ChatValidator.ValidateMessage(msg));
        }

        [Fact]
        public void ValidateMessage_MessageTooLong_ThrowsValidationException()
        {
            string longMsg = new string('A', 256);
            var msg = new ChatMessageData { Nickname = "User", Message = longMsg };

            var ex = Assert.Throws<ValidationException>(() => ChatValidator.ValidateMessage(msg));
            Assert.Contains("255", ex.Message); 
        }
    }
}