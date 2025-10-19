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
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
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

                var session = OperationContext.Current.GetCallbackChannel<ILoginCallback>();
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
    }
}
