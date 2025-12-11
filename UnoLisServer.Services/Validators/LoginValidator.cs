using System;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;

namespace UnoLisServer.Services.Validators
{
    public static class LoginValidator
    {
        private const int HoursInDay = 24;

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
            IPlayerRepository playerRepository = new PlayerRepository();
            var player = playerRepository.GetPlayerWithDetailsAsync(credentials.Nickname).Result;
            if (player == null || player.idPlayer == 0)
            {
                throw new ValidationException(MessageCode.PlayerNotFound,
                    $"No se encontró al jugador {credentials.Nickname}.");
            }

            var account = player.Account.FirstOrDefault();
            if (account == null || account.idAccount == 0)
            {
                throw new ValidationException(MessageCode.PlayerNotFound,
                    $"No se encontró la cuenta para el jugador {credentials.Nickname}.");
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

        public static BanInfo IsPlayerBanned(string nickname)
        {
            ISanctionRepository sanctionRepository = new SanctionRepository();
            IPlayerRepository playerRepository = new PlayerRepository();

            var player = playerRepository.GetPlayerWithDetailsAsync(nickname).Result;
            if (player == null || player.idPlayer == 0)
            {
                throw new ValidationException(MessageCode.PlayerNotFound,
                    $"No se encontró al jugador {nickname}.");
            }

            var activeSanction = sanctionRepository.GetActiveSanction(player.idPlayer);
            if (activeSanction != null && activeSanction.idSanction > 0)
            {
                return MapSanctionToBanInfo(activeSanction);
            }
            return new BanInfo();
        }

        private static BanInfo MapSanctionToBanInfo(Sanction sanction)
        {
            var remainingTime = sanction.sanctionEndDate.Value - DateTime.UtcNow;
            var formattedTime = remainingTime.TotalHours >= HoursInDay
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