using System;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Services.Validators;
using Xunit;

namespace UnoLisServer.Test.ValidatorsTest
{
    public class ProfileEditValidatorTest
    {
        [Fact]
        public void ValidateFormats_AllValidData_ShouldNotThrowException()
        {
            var data = new ProfileData
            {
                Nickname = "ValidUser",
                Email = "valid@test.com",
                Password = "StrongPass1!",
                FacebookUrl = "https://www.facebook.com/user",
                InstagramUrl = "https://instagram.com/user",
                TikTokUrl = "http://tiktok.com/@user"
            };

            ProfileEditValidator.ValidateProfileFormats(data);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateFormats_EmptyEmail_ShouldThrowInvalidEmail(string email)
        {
            var data = new ProfileData { Nickname = "User", Email = email };

            var ex = Assert.Throws<ValidationException>(() => ProfileEditValidator.ValidateProfileFormats(data));
            Assert.Equal(MessageCode.InvalidEmailFormat, ex.ErrorCode);
        }

        [Theory]
        [InlineData("correoinvalido")]
        [InlineData("correo@sinpunto")]
        [InlineData("@dominio.com")]
        [InlineData("correo.com")]
        public void ValidateFormats_BadFormatEmail_ShouldThrowInvalidEmail(string email)
        {
            var data = new ProfileData { Nickname = "User", Email = email };

            var ex = Assert.Throws<ValidationException>(() => ProfileEditValidator.ValidateProfileFormats(data));
            Assert.Equal(MessageCode.InvalidEmailFormat, ex.ErrorCode);
        }

        [Theory]
        [InlineData("short")]           
        [InlineData("onlylowercase1!")] 
        [InlineData("ONLYUPPERCASE1!")] 
        [InlineData("NoSpecialChar1")]  
        [InlineData("NoNumbers!")] 
        public void ValidateFormats_WeakPassword_ShouldThrowWeakPassword(string password)
        {
            var data = new ProfileData { Nickname = "User", Email = "a@a.com", Password = password };

            var ex = Assert.Throws<ValidationException>(() => ProfileEditValidator.ValidateProfileFormats(data));
            Assert.Equal(MessageCode.WeakPassword, ex.ErrorCode);
        }

        [Theory]
        [InlineData("ftp://facebook.com")] 
        [InlineData("www.facebook.com")]
        public void ValidateFormats_InvalidProtocolUrl_ShouldThrowInvalidUrl(string url)
        {
            var data = new ProfileData { Nickname = "User", Email = "a@a.com", FacebookUrl = url };

            var ex = Assert.Throws<ValidationException>(() => ProfileEditValidator.ValidateProfileFormats(data));
            Assert.Equal(MessageCode.InvalidUrlFormat, ex.ErrorCode);
        }

        [Theory]
        [InlineData("https://google.com")] 
        [InlineData("https://twitter.com")] 
        public void ValidateFormats_WrongDomainFacebook_ShouldThrowInvalidUrl(string url)
        {
            var data = new ProfileData { Nickname = "User", Email = "a@a.com", FacebookUrl = url };

            var ex = Assert.Throws<ValidationException>(() => ProfileEditValidator.ValidateProfileFormats(data));
            Assert.Contains("Facebook", ex.Message);
        }

        [Fact]
        public void ValidateFormats_WrongDomainInstagram_ShouldThrowInvalidUrl()
        {
            var data = new ProfileData { Nickname = "User", Email = "a@a.com", InstagramUrl = "https://facebook.com" };

            var ex = Assert.Throws<ValidationException>(() => ProfileEditValidator.ValidateProfileFormats(data));
            Assert.Contains("Instagram", ex.Message);
        }
    }
}