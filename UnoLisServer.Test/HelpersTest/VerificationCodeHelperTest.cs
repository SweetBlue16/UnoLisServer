using System;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using Xunit;

namespace UnoLisServer.Test.HelpersTest
{
    public class VerificationCodeHelperTest
    {
        private readonly IVerificationCodeHelper _helper;

        public VerificationCodeHelperTest()
        {
            _helper = VerificationCodeHelper.Instance;
        }

        [Fact]
        public void TestGenerateAndStoreCodeReturnsSixDigitCode()
        {
            string email = "gen@test.com";
            string code = _helper.GenerateAndStoreCode(email, CodeType.EmailVerification);

            Assert.Equal(6, code.Length);
            Assert.True(int.TryParse(code, out _));
        }

        [Fact]
        public void TestValidateCodeCorrectCodeReturnsTrue()
        {
            string email = "valid@test.com";
            string code = _helper.GenerateAndStoreCode(email, CodeType.EmailVerification);

            var request = new CodeValidationRequest
            {
                Identifier = email,
                Code = code,
                CodeType = (int)CodeType.EmailVerification,
                Consume = false 
            };

            bool result = _helper.ValidateCode(request);

            Assert.True(result);
        }

        [Fact]
        public void TestValidateCodeWrongCodeReturnsFalse()
        {
            string email = "wrong@test.com";
            _helper.GenerateAndStoreCode(email, CodeType.EmailVerification);

            var request = new CodeValidationRequest
            {
                Identifier = email,
                Code = "000000", 
                CodeType = (int)CodeType.EmailVerification
            };

            bool result = _helper.ValidateCode(request);
            Assert.False(result);
        }

        [Fact]
        public void TestValidateCodeWrongTypeReturnsFalse()
        {
            string email = "type@test.com";
            string code = _helper.GenerateAndStoreCode(email, CodeType.EmailVerification);

            var request = new CodeValidationRequest
            {
                Identifier = email,
                Code = code,
                CodeType = (int)CodeType.PasswordReset
            };

            bool result = _helper.ValidateCode(request);
            Assert.False(result);
        }

        [Fact]
        public void TestValidateCodeConsumeTrueRemovesCode()
        {
            string email = "consume@test.com";
            string code = _helper.GenerateAndStoreCode(email, CodeType.EmailVerification);

            var request = new CodeValidationRequest
            {
                Identifier = email,
                Code = code,
                CodeType = (int)CodeType.EmailVerification,
                Consume = true
            };

            bool firstResult = _helper.ValidateCode(request);
            bool secondResult = _helper.ValidateCode(request); 

            Assert.True(firstResult, "La primera validación debió pasar");
            Assert.False(secondResult, "La segunda validación debió fallar porque el código fue consumido");
        }

        [Fact]
        public void TestValidateCodeNonExistentIdentifierReturnsFalse()
        {
            var request = new CodeValidationRequest
            {
                Identifier = "ghost@test.com",
                Code = "123456",
                CodeType = (int)CodeType.EmailVerification
            };

            Assert.False(_helper.ValidateCode(request));
        }

        [Fact]
        public void TestCanRequestCodeFirstTimeReturnsTrue()
        {
            Assert.True(_helper.CanRequestCode("fresh@limit.com", CodeType.EmailVerification));
        }

        [Fact]
        public void TestCanRequestCodeImmediatelyAfterGenerationReturnsFalse()
        {
            string email = "spam@limit.com";
            _helper.GenerateAndStoreCode(email, CodeType.EmailVerification);

            bool canRequest = _helper.CanRequestCode(email, CodeType.EmailVerification);

            Assert.False(canRequest);
        }
    }
}