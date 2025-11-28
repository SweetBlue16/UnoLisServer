using UnoLisServer.Common.Exceptions;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Common.Enums;
using UnoLisServer.Data;
using System.Linq;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Data.Repositories;
using System;

namespace UnoLisServer.Services.Validators
{
    public static class LoginValidator
    {
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
            using (var context = new UNOContext())
            {
                var account = context.Account
                    .Include("Player")
                    .FirstOrDefault(a => a.Player.nickname == credentials.Nickname);
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

        public static BanInfo IsPlayerBanned(string nickname)
        {
            ISanctionRepository sanctionRepository = new SanctionRepository();
            IPlayerRepository playerRepository = new PlayerRepository();
            var player = playerRepository.GetPlayerWithDetailsAsync(nickname).Result;
            var activeSanction = sanctionRepository.GetActiveSanction(player.idPlayer);
            if (activeSanction != null)
            {
                return MapSanctionToBanInfo(activeSanction);
            }
            return new BanInfo();
        }

        private static BanInfo MapSanctionToBanInfo(Sanction sanction)
        {
            var remainingTime = sanction.sanctionEndDate.Value - DateTime.UtcNow;
            var formattedTime = remainingTime.TotalHours >= 24
                ? $"{(int)remainingTime.TotalDays}d {remainingTime.Hours}h"
                : $"{(int)remainingTime.TotalHours}h {remainingTime.Minutes}m";

            return new BanInfo
            {
                Reason = sanction.sanctionDescription,
                EndDate = sanction.sanctionEndDate.Value,
                RemainingHours = remainingTime.TotalHours,
                FormattedTimeRemaining = formattedTime
            };
        }
    }
}