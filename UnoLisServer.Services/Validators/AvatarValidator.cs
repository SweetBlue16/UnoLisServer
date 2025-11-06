using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Data;
using System.Data.Entity;

namespace UnoLisServer.Services.Validators
{
    public static class AvatarValidator
    {
        public static Player ValidatePlayerExists(UNOContext context, string nickname)
        {
            var player = context.Player
                .Include(p => p.AvatarsUnlocked)
                .FirstOrDefault(p => p.nickname == nickname);
            if (player == null)
            {
                throw new ValidationException(MessageCode.PlayerNotFound, $"El jugador '{nickname}' no fue encontrado.");
            }
            return player;
        }

        public static void ValidateAvatarIsUnlocked(Player player, int newAvatarId)
        {
            if (newAvatarId == 1)
            {
                return;
            }

            bool isUnlocked = player.AvatarsUnlocked.Any(au => au.Avatar_idAvatar == newAvatarId);
            if (!isUnlocked)
            {
                throw new ValidationException(MessageCode.InvalidAvatarSelection, $"El avatar con ID '{newAvatarId}' no está desbloqueado para el jugador '{player.nickname}'.");
            }
        }
    }
}
