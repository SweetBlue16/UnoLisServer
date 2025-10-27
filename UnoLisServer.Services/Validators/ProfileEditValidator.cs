using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Data;

namespace UnoLisServer.Services.Validators
{
    public static class ProfileEditValidator
    {
        private static UNOContext _context;
        private static readonly Regex _strongPasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\-_])[A-Za-z\d@$!%*?&\-_]{8,16}$");

        public static void ValidateProfileUpdate(ProfileData data)
        {
            _context = new UNOContext();

            if (data == null || string.IsNullOrWhiteSpace(data.Nickname))
            {
                throw new ValidationException(MessageCode.InvalidData,
                    "Los datos del perfil no pueden estar vacíos.");
            }

            var player = _context.Player.FirstOrDefault(p => p.nickname == data.Nickname);
            if (player == null)
            {
                throw new ValidationException(MessageCode.PlayerNotFound,
                    $"El jugador con nickname '{data.Nickname}' no fue encontrado.");
            }

            var account = _context.Account.FirstOrDefault(a => a.Player_idPlayer == player.idPlayer);
            if (account == null)
            {
                throw new ValidationException(MessageCode.PlayerNotFound,
                    $"La cuenta del jugador con nickname '{data.Nickname}' no fue encontrada.");
            }

            ValidateEmail(data.Email);
            ValidatePasswordStrength(data.Password);
            if (PasswordHelper.VerifyPassword(data.Password, account.password))
            {
                throw new ValidationException(MessageCode.SamePassword,
                    "La nueva contraseña no puede ser igual a la anterior.");
            }

            ValidateSocialMediaLink(data.FacebookUrl, "facebook.com", "Facebook");
            ValidateSocialMediaLink(data.InstagramUrl, "instagram.com", "Instagram");
            ValidateSocialMediaLink(data.TikTokUrl, "tiktok.com", "TikTok");
        }

        private static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ValidationException(MessageCode.InvalidEmailFormat,
                    $"El formato del email '{email}' no es válido.");
            }

            try
            {
                var mailAddress = new MailAddress(email);
            }
            catch (FormatException)
            {
                throw new ValidationException(MessageCode.InvalidEmailFormat,
                    $"El formato del email '{email}' no es válido.");
            }
        }

        private static void ValidateSocialMediaLink(string url, string expectedDomain, string platformName)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                throw new ValidationException(MessageCode.InvalidUrlFormat,
                    $"El formato del enlace de {platformName} '{url}' no es válido.");
            }
            else if (!uriResult.Host.Contains(expectedDomain))
            {
                throw new ValidationException(MessageCode.InvalidUrlFormat,
                    $"El enlace de {platformName} '{url}' no corresponde a la plataforma esperada.");
            }
        }

        private static void ValidatePasswordStrength(string password)
        {
            if (!string.IsNullOrWhiteSpace(password) && !_strongPasswordRegex.IsMatch(password))
            {
                throw new ValidationException(MessageCode.WeakPassword,
                    "La contraseña debe tener entre 8 y 16 caracteres, incluir al menos una letra mayúscula, una letra minúscula, un número y un carácter especial.");
            }
        }
    }
}
