using System;
using UnoLisServer.Common.Helpers;
using Xunit;

namespace UnoLisServer.Test.HelpersTest
{
    public class PasswordHelperTest
    {
        [Fact]
        public void HashPassword_SameInput_ReturnsSameHash()
        {
            string input = "MySecretPass123";
            string hash1 = PasswordHelper.HashPassword(input);
            string hash2 = PasswordHelper.HashPassword(input);

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void HashPassword_DifferentInput_ReturnsDifferentHash()
        {
            string hash1 = PasswordHelper.HashPassword("PasswordA");
            string hash2 = PasswordHelper.HashPassword("PasswordB");

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void HashPassword_ReturnsCorrectFormat()
        {
            string hash = PasswordHelper.HashPassword("Test");

            Assert.NotNull(hash);
            Assert.Equal(64, hash.Length);

            Assert.Matches("^[a-f0-9]+$", hash);
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            string password = "SecurePassword!";
            string hash = PasswordHelper.HashPassword(password);

            bool result = PasswordHelper.VerifyPassword(password, hash);

            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            string password = "SecurePassword!";
            string hash = PasswordHelper.HashPassword(password);

            bool result = PasswordHelper.VerifyPassword("WrongOne", hash);

            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_CaseSensitive_ReturnsFalse()
        {
            string password = "Password";
            string hash = PasswordHelper.HashPassword(password);

            bool result = PasswordHelper.VerifyPassword("password", hash);

            Assert.False(result);
        }

        [Fact]
        public void HashPassword_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => PasswordHelper.HashPassword(null));
        }
    }
}