using System;
using UnoLisServer.Services.Validators;
using Xunit;

namespace UnoLisServer.Test
{
    public class FriendsValidatorTest
    {
        [Fact]
        public void ValidateNicknames_BothValid_DoesNotThrow()
        {
            string nick1 = "AlphaUser";
            string nick2 = "BetaUser";

            FriendsValidator.ValidateNicknames(nick1, nick2);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateNicknames_InvalidRequester_ThrowsArgumentException(string invalidNick)
        {
            string validTarget = "ValidUser";

            var ex = Assert.Throws<ArgumentException>(() =>
                FriendsValidator.ValidateNicknames(invalidNick, validTarget));

            Assert.Contains("no pueden estar vacíos", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateNicknames_InvalidTarget_ThrowsArgumentException(string invalidNick)
        {
            string validRequester = "ValidUser";

            var ex = Assert.Throws<ArgumentException>(() =>
                FriendsValidator.ValidateNicknames(validRequester, invalidNick));

            Assert.Contains("no pueden estar vacíos", ex.Message);
        }

        [Fact]
        public void ValidateNicknames_BothInvalid_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                FriendsValidator.ValidateNicknames("", null));
        }
    }
}