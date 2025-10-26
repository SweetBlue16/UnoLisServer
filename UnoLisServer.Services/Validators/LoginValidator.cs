using UnoLisServer.Common.Exceptions;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Common.Enums;
using UnoLisServer.Data;
using System.Linq;
using UnoLisServer.Common.Helpers;

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
        }
    }
}