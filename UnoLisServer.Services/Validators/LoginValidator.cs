using UnoLisServer.Common.Exceptions;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Common.Enums;
using UnoLisServer.Data;
using System.Linq;
using UnoLisServer.Common.Helpers;
using System;

namespace UnoLisServer.Services.Validators
{
    public static class LoginValidator
    {
        private static UNOContext _context;

        public static void ValidateCredentials(AuthCredentials credentials)
        {
            if (credentials == null ||
                string.IsNullOrWhiteSpace(credentials.Nickname) ||
                string.IsNullOrWhiteSpace(credentials.Password))
            {
                throw new ValidationException(MessageCode.EmptyFields,
                    $"Credenciales vacías para {credentials.Nickname}.");
            }
        }

        public static void AuthenticatePlayer(AuthCredentials credentials)
        {
            ValidateCredentials(credentials);
            _context = new UNOContext();

            var account = _context.Account.FirstOrDefault(a => a.Player.nickname == credentials.Nickname);
            if (account == null)
            {
                throw new ValidationException(MessageCode.PlayerNotFound,
                    $"No se encontró al jugador {credentials.Nickname}.");
            }

            bool isPasswordValid = PasswordHelper.VerifyPassword(credentials.Password, account.password);
            if (!isPasswordValid)
            {
                throw new ValidationException(MessageCode.InvalidCredentials,
                    $"Credenciales inválidas para {credentials.Nickname}");
            }

            if (SessionManager.IsOnline(account.Player.nickname))
            {
                throw new ValidationException(MessageCode.DuplicateSession,
                    $"El jugador {credentials.Nickname} ya tiene una sesión activa.");
            }
            CheckActiveSanction(account.Player);
        }

        private static void CheckActiveSanction(Player player)
        {
            _context.Entry(player).Collection(p => p.Sanction).Load();

            var activeSanction = player.Sanction
                .Where(s => s.sanctionEndDate > DateTime.UtcNow)
                .OrderByDescending(s => s.sanctionEndDate)
                .FirstOrDefault();

            if (activeSanction != null)
            {
                var remainingTime = activeSanction.sanctionEndDate.Value - DateTime.UtcNow;
                string timeString = remainingTime.TotalHours >= 24
                    ? $"{(int)remainingTime.TotalDays} días"
                    : $"{(int)remainingTime.TotalHours}h {remainingTime.Minutes}m";
                string message = $"Cuenta suspendida. Restante: {timeString}.";

                throw new ValidationException(MessageCode.PlayerBanned, message);
            }
        }
    }
}