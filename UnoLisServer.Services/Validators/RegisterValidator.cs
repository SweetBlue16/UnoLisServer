using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Data;

namespace UnoLisServer.Services.Validators
{
    public static class RegisterValidator
    {
        private static UNOContext _context;

        public static void ValidateRegistrationData(RegistrationData data)
        {
            if (data == null ||
                string.IsNullOrWhiteSpace(data.Email) ||
                string.IsNullOrWhiteSpace(data.Password) ||
                string.IsNullOrWhiteSpace(data.Nickname) ||
                string.IsNullOrWhiteSpace(data.FullName))
            {
                throw new ValidationException(MessageCode.EmptyFields,
                    $"Datos de registro inválidos para '{data.Nickname}'.");
            }
        }

        public static void CheckExistingUser(RegistrationData data)
        {
            _context = new UNOContext();

            bool existsPlayer = _context.Player.Any(p => p.nickname == data.Nickname);
            if (existsPlayer)
            {
                throw new ValidationException(MessageCode.NicknameAlreadyTaken,
                    $"El nickname '{data.Nickname}' ya está en uso.");
            }

            bool existsAccount = _context.Account.Any(a => a.email == data.Email);
            if (existsAccount)
            {
                throw new ValidationException(MessageCode.EmailAlreadyRegistered,
                    $"El email '{data.Email}' ya está registrado.");
            }
        }
    }
}
