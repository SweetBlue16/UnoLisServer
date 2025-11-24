using System;
using UnoLisServer.Common.Helpers;
using Xunit;

namespace UnoLisServer.Test.HelpersTest
{
    public class PasswordHelperTest
    {
        [Fact]
        public void TestHashPasswordSameInputReturnsSameHash()
        {
            string input = "MySecretPass123";
            string hash1 = PasswordHelper.HashPassword(input);
            string hash2 = PasswordHelper.HashPassword(input);

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void TestHashPasswordDifferentInputReturnsDifferentHash()
        {
            string hash1 = PasswordHelper.HashPassword("PasswordA");
            string hash2 = PasswordHelper.HashPassword("PasswordB");

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void TestHashPasswordReturnsCorrectFormat()
        {
            string hash = PasswordHelper.HashPassword("Test");

            Assert.NotNull(hash);
            Assert.Equal(64, hash.Length);

            Assert.Matches("^[a-f0-9]+$", hash);
        }

        [Fact]
        public void TestVerifyPasswordCorrectPasswordReturnsTrue()
        {
            string password = "SecurePassword!";
            string hash = PasswordHelper.HashPassword(password);

            bool result = PasswordHelper.VerifyPassword(password, hash);

            Assert.True(result);
        }

        [Fact]
        public void TestVerifyPasswordWrongPasswordReturnsFalse()
        {
            string password = "SecurePassword!";
            string hash = PasswordHelper.HashPassword(password);

            bool result = PasswordHelper.VerifyPassword("WrongOne", hash);

            Assert.False(result);
        }

        [Fact]
        public void TestVerifyPasswordCaseSensitiveReturnsFalse()
        {
            string password = "Password";
            string hash = PasswordHelper.HashPassword(password);

            bool result = PasswordHelper.VerifyPassword("password", hash);

            Assert.False(result);
        }

        [Fact]
        public void TestHashPasswordNullInputThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => PasswordHelper.HashPassword(null));
        }
    }
}