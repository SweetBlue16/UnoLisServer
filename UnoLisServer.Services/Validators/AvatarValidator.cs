using System.Collections.Generic;
using System.Linq;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Services.Validators
{
    public static class AvatarValidator
    {
        public static void ValidateSelection(int newAvatarId, List<PlayerAvatar> unlockedAvatars)
        {
            if (newAvatarId == 1) return;

            if (unlockedAvatars == null || !unlockedAvatars.Any(a => a.AvatarId == newAvatarId))
            {
                throw new ValidationException(MessageCode.InvalidAvatarSelection,
                    $"El avatar con ID {newAvatarId} no está desbloqueado o no existe.");
            }
        }
    }
}