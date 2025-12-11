using System;
using UnoLisServer.Services.Validators;
using Xunit;

namespace UnoLisServer.Test
{
    public class FriendsValidatorTest
    {
        [Fact]
        public void TestValidateNicknamesBothValidDoesNotThrow()
        {
            string nick1 = "AlphaUser";
            string nick2 = "BetaUser";

            FriendsValidator.ValidateNicknames(nick1, nick2);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TestValidateNicknamesInvalidRequesterThrowsArgumentException(string invalidNick)
        {
            string validTarget = "ValidUser";

            var ex = Assert.Throws<ArgumentException>(() =>
                FriendsValidator.ValidateNicknames(invalidNick, validTarget));

            Assert.Contains("can not be empty", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TestValidateNicknamesInvalidTargetThrowsArgumentException(string invalidNick)
        {
            string validRequester = "ValidUser";

            var ex = Assert.Throws<ArgumentException>(() =>
                FriendsValidator.ValidateNicknames(validRequester, invalidNick));

            Assert.Contains("can not be empty", ex.Message);
        }

        [Fact]
        public void TestValidateNicknamesBothInvalidThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                FriendsValidator.ValidateNicknames("", null));
        }
    }
}