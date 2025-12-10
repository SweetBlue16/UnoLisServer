using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Services.Validators
{
    public static class ProfileEditValidator
    {
        private const int ValidEmailPartCount = 2;
        private static readonly Regex _strongPasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\-_])[A-Za-z\d@$!%*?&\-_]{8,16}$");

        public static void ValidateProfileFormats(ProfileData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.Nickname))
            {
                throw new ValidationException(MessageCode.InvalidData, "Los datos del perfil no pueden estar vacíos.");
            }

            ValidateEmail(data.Email);

            if (!string.IsNullOrWhiteSpace(data.Password))
            {
                ValidatePasswordStrength(data.Password);
            }

            ValidateSocialMediaLink(data.FacebookUrl, "facebook.com", "Facebook");
            ValidateSocialMediaLink(data.InstagramUrl, "instagram.com", "Instagram");
            ValidateSocialMediaLink(data.TikTokUrl, "tiktok.com", "TikTok");
        }

        private static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ValidationException(MessageCode.InvalidEmailFormat, "El email es requerido.");
            }

            try
            {
                var mailAddress = new MailAddress(email);

                var parts = mailAddress.Host.Split('.');
                if (parts.Length < ValidEmailPartCount)
                {
                    throw new FormatException("El dominio debe tener al menos un punto.");
                }
            }
            catch (FormatException)
            {
                throw new ValidationException(MessageCode.InvalidEmailFormat, $"El formato del email '{email}' no es válido.");
            }
        }

        private static void ValidateSocialMediaLink(string url, string expectedDomain, string platformName)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                throw new ValidationException(MessageCode.InvalidUrlFormat, $"El formato del enlace de {platformName} no es válido.");
            }
            else if (!uriResult.Host.Contains(expectedDomain))
            {
                throw new ValidationException(MessageCode.InvalidUrlFormat, $"El enlace no corresponde a {platformName}.");
            }
        }

        private static void ValidatePasswordStrength(string password)
        {
            if (!_strongPasswordRegex.IsMatch(password))
            {
                throw new ValidationException(MessageCode.WeakPassword, "La contraseña no cumple con los requisitos de seguridad.");
            }
        }
    }
}