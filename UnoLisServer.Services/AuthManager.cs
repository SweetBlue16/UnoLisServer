using System;
using System.Linq;
using System.ServiceModel;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Services;

namespace UnoLisServer.Services
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class AuthManager : IAuthManager
    {
        private readonly UNOContext _context;
        private readonly IAuthCallback _callback;

        public AuthManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<IAuthCallback>();
        }

        public void Login(AuthCredentials credentials)
        {
            try
            {
                Logger.Log($"Intentando login para {credentials.Nickname}...");

                var account = _context.Account.FirstOrDefault(a => a.email == credentials.Nickname);
                if (account == null)
                {
                    _callback.LoginResponse(false, "Usuario no encontrado.");
                    return;
                }

                bool valid = PasswordHelper.VerifyPassword(credentials.Password, account.password);
                if (!valid)
                {
                    _callback.LoginResponse(false, "Contraseña incorrecta.");
                    return;
                }

                // Registrar sesión
                var session = OperationContext.Current.GetCallbackChannel<IAuthCallback>();
                SessionManager.AddSession(credentials.Nickname, session);

                _callback.LoginResponse(true, "Inicio de sesión exitoso.");
                Logger.Log($"Usuario {credentials.Nickname} inició sesión correctamente.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error en Login({credentials.Nickname}): {ex.Message}");
                _callback.LoginResponse(false, "Error interno del servidor.");
            }
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

        public void ConfirmCode(string email, string code)
        {
            // De momento, simulamos validación
            Logger.Log($"Confirmando código para {email}: {code}");
            _callback.ConfirmationResponse(true);
        }
    }
}
