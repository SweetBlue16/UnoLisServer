using System;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;

namespace UnoLisServer.Services.Validators
{
    public static class LogoutValidator
    {
        public static void ValidateLogout(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                throw new ValidationException(MessageCode.InvalidData,
                    $"Nickname inválido para '{nickname}'.");
            }
        }
    }
}
