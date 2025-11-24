using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Services.Validators;
using Xunit;

namespace UnoLisServer.Test.ValidatorsTest
{
    public class RegisterValidatorTest
    {
        [Fact]
        public void TestValidateFormatsAllFieldsValidShouldPass()
        {
            var data = new RegistrationData
            {
                Email = "valid@test.com",
                Password = "StrongPassword1!",
                Nickname = "ValidNick",
                FullName = "Valid Name"
            };

            RegisterValidator.ValidateFormats(data);
        }

        [Fact]
        public void TestValidateFormatsNullObjectShouldThrowEmptyFields()
        {
            var ex = Assert.Throws<ValidationException>(() => RegisterValidator.ValidateFormats(null));
            Assert.Equal(MessageCode.EmptyFields, ex.ErrorCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TestValidateFormatsInvalidEmailShouldThrow(string email)
        {
            var data = new RegistrationData
            {
                Email = email,
                Password = "P",
                Nickname = "N",
                FullName = "F"
            };

            var ex = Assert.Throws<ValidationException>(() => RegisterValidator.ValidateFormats(data));
            Assert.Equal(MessageCode.EmptyFields, ex.ErrorCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TestValidateFormatsInvalidPasswordShouldThrow(string password)
        {
            var data = new RegistrationData
            {
                Email = "e@e.com",
                Password = password,
                Nickname = "N",
                FullName = "F"
            };

            var ex = Assert.Throws<ValidationException>(() => RegisterValidator.ValidateFormats(data));
            Assert.Equal(MessageCode.EmptyFields, ex.ErrorCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TestValidateFormatsInvalidNicknameShouldThrow(string nickname)
        {
            var data = new RegistrationData
            {
                Email = "e@e.com",
                Password = "P",
                Nickname = nickname,
                FullName = "F"
            };

            var ex = Assert.Throws<ValidationException>(() => RegisterValidator.ValidateFormats(data));
            Assert.Equal(MessageCode.EmptyFields, ex.ErrorCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TestValidateFormatsInvalidFullNameShouldThrow(string fullName)
        {
            var data = new RegistrationData
            {
                Email = "e@e.com",
                Password = "P",
                Nickname = "N",
                FullName = fullName
            };

            var ex = Assert.Throws<ValidationException>(() => RegisterValidator.ValidateFormats(data));
            Assert.Equal(MessageCode.EmptyFields, ex.ErrorCode);
        }
    }
}