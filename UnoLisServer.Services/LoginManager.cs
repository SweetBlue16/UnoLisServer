using System;
using System.Collections.Generic;
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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LoginManager : ILoginManager
    {
        private readonly UNOContext _context;
        private readonly ILoginCallback _callback;
        public LoginManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<ILoginCallback>();
        }

        public void Login(AuthCredentials credentials)
        {
            try
            {
                if (credentials == null || string.IsNullOrWhiteSpace(credentials.Nickname) || string.IsNullOrWhiteSpace(credentials.Password))
                {
                    _callback.LoginResponse(false, "Datos de inicio de sesión inválidos.");
                    return;
                }
                Logger.Log($"Intentando inicio de sesión para '{credentials.Nickname}'...");

                var account = _context.Account.FirstOrDefault(a => a.Player.nickname == credentials.Nickname);
                if (account == null)
                {
                    _callback.LoginResponse(false, "El usuario no existe.");
                    Logger.Log($"Usuario '{credentials.Nickname}' no encontrado.");
                    return;
                }

                bool isPasswordValid = PasswordHelper.VerifyPassword(credentials.Password, account.password);
                if (!isPasswordValid)
                {
                    _callback.LoginResponse(false, "Contraseña incorrecta.");
                    Logger.Log($"Contraseña incorrecta para '{credentials.Nickname}'.");
                    return;
                }

                if (SessionManager.IsOnline(account.Player.nickname))
                {
                    _callback.LoginResponse(false, "El usuario ya está conectado.");
                    Logger.Log($"Sesión duplicada detectada para '{credentials.Nickname}'");
                    return;
                }

                var session = OperationContext.Current.GetCallbackChannel<ILoginCallback>();
                SessionManager.AddSession(account.Player.nickname, session);

                _callback.LoginResponse(true, "Inicio de sesión exitoso.");
                Logger.Log($"Usuario '{account.Player.nickname}' inició sesión correctamente.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Fallo en el Login('{credentials?.Nickname ?? "null"}'): {ex}");
                _callback.LoginResponse(false, "Error interno del servidor durante el inicio de sesión.");
            }
        }
    }
}
