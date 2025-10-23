using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Common.Enums;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LogoutManager : ILogoutManager
    {
        private readonly ILogoutCallback _callback;
        private ServiceResponse<object> _response;

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
                    _response = new ServiceResponse<object>(false, MessageCode.InvalidData);
                    _callback.LogoutResponse(_response);
                    return;
                }

                if (!SessionManager.IsOnline(nickname))
                {
                    _response = new ServiceResponse<object>(false, MessageCode.UserNotConnected);
                    _callback.LogoutResponse(_response);
                    Logger.Log($"Intento de logout inválido para '{nickname}'.");
                    return;
                }

                SessionManager.RemoveSession(nickname);
                _response = new ServiceResponse<object>(true, MessageCode.LogoutSuccessful);
                _callback.LogoutResponse(_response);
                Logger.Log($"Usuario '{nickname}' cerró sesión correctamente.");
            }
            catch (CommunicationException communicationEx)
            {
                Logger.Log($"Error de comunicación durante logout para '{nickname}'. Error: {communicationEx.Message}");
            }
            catch (TimeoutException timeoutEx)
            {
                Logger.Log($"Tiempo de espera agotado durante el logout para '{nickname}'. Error: {timeoutEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error durante el logout para '{nickname}': {ex.Message}");
                _response = new ServiceResponse<object>(false, MessageCode.LogoutInternalError);
                _callback.LogoutResponse(_response);
            }
        }
    }
}
