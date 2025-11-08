using System.Linq;
using UnoLisServer.Data;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Common.Enums;
using System.Data.Entity;

namespace UnoLisServer.Services.Validators
{
    public static class ProfileViewValidator
    {
        private static UNOContext _context;

        public static Player ValidateProfileData(string nickname)
        {
            _context = new UNOContext();

            var player = _context.Player
                .Include(p => p.Account)
                .Include(p => p.PlayerStatistics)
                .Include(p => p.SocialNetwork)
                .Include(p => p.AvatarsUnlocked.Select(au => au.Avatar))
                .FirstOrDefault(p => p.nickname == nickname);

            if (player == null)
            {
                throw new ValidationException(MessageCode.PlayerNotFound,
                    $"No se encontró el perfil para '{nickname}'.");
            }
            return player;
        }
    }
}
