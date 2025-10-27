using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Data;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Common.Enums;

namespace UnoLisServer.Services.Validators
{
    public static class ProfileViewValidator
    {
        private static UNOContext _context;

        public static Player ValidateProfileData(string nickname)
        {
            _context = new UNOContext();

            var player = _context.Player.FirstOrDefault(p => p.nickname == nickname);
            if (player == null)
            {
                throw new ValidationException(MessageCode.PlayerNotFound,
                    $"No se encontró el perfil para '{nickname}'.");
            }
            return player;
        }
    }
}
