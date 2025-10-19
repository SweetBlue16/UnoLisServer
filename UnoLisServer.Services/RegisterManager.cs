using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;

namespace UnoLisServer.Services
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class RegisterManager : IRegisterManager
    {
        private readonly UNOContext _context;
        private readonly IRegisterCallback _callback;

        public RegisterManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<IRegisterCallback>();
        }

        public void Register(RegistrationData data)
        {
            try
            {
                Logger.Log($"Intentando registrar cuenta {data.Email}...");

                if (_context.Account.Any(a => a.email == data.Email))
                {
                    _callback.RegisterResponse(false, "El correo ya está registrado.");
                    return;
                }

                var newPlayer = new Player { nickname = data.Email.Split('@')[0] };
                _context.Player.Add(newPlayer);
                _context.SaveChanges();

                var newAccount = new Account
                {
                    email = data.Email,
                    password = PasswordHelper.HashPassword(data.Password),
                    Player_idPlayer = newPlayer.idPlayer
                };

                _context.Account.Add(newAccount);
                _context.SaveChanges();

                _callback.RegisterResponse(true, "Registro completado exitosamente.");
                Logger.Log($"Registro exitoso para {data.Email}.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error en Register({data.Email}): {ex.Message}");
                _callback.RegisterResponse(false, "Error interno del servidor.");
            }
        }
    }
}
