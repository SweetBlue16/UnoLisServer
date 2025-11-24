using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Services.Validators;
using Xunit;

namespace UnoLisServer.Test.ValidatorsTest
{
    public class ConfirmationValidatorTest
    {
        [Fact]
        public void TestValidateInputWithValidDataShouldPass()
        {
            ConfirmationValidator.ValidateInput("test@test.com", "123456");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TestValidateInputWithInvalidEmailShouldThrow(string email)
        {
            var ex = Assert.Throws<ValidationException>(() => ConfirmationValidator.ValidateInput(email, "123456"));
            Assert.Equal(MessageCode.EmptyFields, ex.ErrorCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TestValidateInputWithInvalidCodeShouldThrow(string code)
        {
            var ex = Assert.Throws<ValidationException>(() => ConfirmationValidator.ValidateInput("a@a.com", code));
            Assert.Equal(MessageCode.EmptyFields, ex.ErrorCode);
        }

        [Fact]
        public void TestValidateResendInputWithValidEmailShouldPass()
        {
            ConfirmationValidator.ValidateResendInput("test@test.com");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TestValidateResendInputWithInvalidEmailShouldThrow(string email)
        {
            var ex = Assert.Throws<ValidationException>(() => ConfirmationValidator.ValidateResendInput(email));
            Assert.Equal(MessageCode.EmptyFields, ex.ErrorCode);
        }
    }
}