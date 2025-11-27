using System;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Services.Validators;
using Xunit;

namespace UnoLisServer.Test
{
    public class LobbyValidatorTest
    {
        [Fact]
        public void TestValidateSettingsNullSettingsThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => LobbyValidator.ValidateSettings(null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TestValidateSettingsInvalidHostNicknameThrowsArgumentException(string invalidNick)
        {
            var settings = new MatchSettings { HostNickname = invalidNick, MaxPlayers = 4 };

            var ex = Assert.Throws<ArgumentException>(() => LobbyValidator.ValidateSettings(settings));
            Assert.Contains("Host nickname is required", ex.Message);
        }

        [Theory]
        [InlineData(0)]  
        [InlineData(1)]  
        [InlineData(5)] 
        [InlineData(-10)]
        public void TestValidateSettingsInvalidPlayerCountThrowsArgumentException(int invalidCount)
        {
            var settings = new MatchSettings { HostNickname = "Host", MaxPlayers = invalidCount };

            var ex = Assert.Throws<ArgumentException>(() => LobbyValidator.ValidateSettings(settings));
            Assert.Contains("between 2 and 4", ex.Message);
        }

        [Theory]
        [InlineData(2)] 
        [InlineData(3)] 
        [InlineData(4)] 
        public void TestValidateSettingsValidBoundariesDoesNotThrow(int validCount)
        {
            var settings = new MatchSettings { HostNickname = "Host", MaxPlayers = validCount };
            LobbyValidator.ValidateSettings(settings);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TestValidateJoinRequestInvalidLobbyCodeThrowsArgumentException(string invalidCode)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                LobbyValidator.ValidateJoinRequest(invalidCode, "ValidNick"));

            Assert.Contains("Lobby code required", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TestValidateJoinRequestInvalidNicknameThrowsArgumentException(string invalidNick)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                LobbyValidator.ValidateJoinRequest("VALIDCODE", invalidNick));

            Assert.Contains("Nickname required", ex.Message);
        }

        [Fact]
        public void TestValidateJoinRequestValidInputsDoesNotThrow()
        {
            LobbyValidator.ValidateJoinRequest("ABCDE", "PlayerOne");
        }
    }
}