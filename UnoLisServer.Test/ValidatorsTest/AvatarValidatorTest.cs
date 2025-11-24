using System.Collections.Generic;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Services.Validators;
using Xunit;

namespace UnoLisServer.Test.ValidatorsTest
{
    public class AvatarValidatorTest
    {
        [Fact]
        public void TestValidateSelectionDefaultAvatarAlwaysPasses()
        {
            AvatarValidator.ValidateSelection(1, new List<PlayerAvatar>());
        }

        [Fact]
        public void TestValidateSelectionUnlockedAvatarPasses()
        {
            var list = new List<PlayerAvatar> { new PlayerAvatar { AvatarId = 50 } };
            AvatarValidator.ValidateSelection(50, list);
        }

        [Fact]
        public void TestValidateSelectionLockedAvatarThrowsException()
        {
            var list = new List<PlayerAvatar> { new PlayerAvatar { AvatarId = 50 } };
            var ex = Assert.Throws<ValidationException>(() => AvatarValidator.ValidateSelection(99, list));
            Assert.Equal(MessageCode.InvalidAvatarSelection, ex.ErrorCode);
        }
    }
}