using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Common.Helpers;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LogoutManager : ILogoutManager
    {
        private readonly ILogoutCallback _callback;

        public LogoutManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<ILogoutCallback>();
        }

        public void Logout(string nickname)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nickname))
                {
                    _callback.LogoutResponse(false, "Nickname inválido.");
                    return;
                }

                if (!SessionManager.IsOnline(nickname))
                {
                    _callback.LogoutResponse(false, $"El usuario '{nickname}' no está conectado.");
                    Logger.Log($"Intento de logout inválido para '{nickname}'.");
                    return;
                }

                SessionManager.RemoveSession(nickname);
                _callback.LogoutResponse(true, $"El usuario '{nickname}' cerró sesión correctamente.");
                Logger.Log($"Usuario '{nickname}' cerró sesión correctamente.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error durante el logout para '{nickname}': {ex.Message}");
                _callback.LogoutResponse(false, "Error interno del servidor durante el logout.");
            }
        }
    }
}
