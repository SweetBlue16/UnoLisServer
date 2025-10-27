using System;
using System.ServiceModel;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Common.Enums;
using UnoLisServer.Services.Validators;
using UnoLisServer.Common.Exceptions;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LogoutManager : ILogoutManager
    {
        private readonly ILogoutCallback _callback;
        private ResponseInfo<object> _responseInfo;

        public LogoutManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<ILogoutCallback>();
        }

        public void Logout(string nickname)
        {
            string userNickname = nickname ?? "Unknown";
            try
            {
                Logger.Log($"[INFO] Intentando cerrar la sesión de '{userNickname}'...");
                LogoutValidator.ValidateLogout(userNickname);

                SessionManager.RemoveSession(nickname);
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.LogoutSuccessful,
                    true,
                    $"[INFO] Jugador '{userNickname}' cerró sesión correctamente."
                );
            }
            catch (ValidationException validationEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    validationEx.ErrorCode,
                    false,
                    $"[WARNING] Validación fallida durante el logout para '{userNickname}'. Error: {validationEx.Message}"
                );
            }
            catch (CommunicationException communicationEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.ConnectionFailed,
                    false,
                    $"[ERROR] Comunicación durante el logout para '{userNickname}'. Error: {communicationEx.Message}"
                );
            }
            catch (TimeoutException timeoutEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.Timeout,
                    false,
                    $"[ERROR] Tiempo de espera agotado durante el logout para '{userNickname}'. Error: {timeoutEx.Message}"
                );
            }
            catch (Exception ex)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.LogoutInternalError,
                    false,
                    $"[FATAL] Error inesperado durante el logout para '{userNickname}'. Error: {ex.Message}"
                );
            }
            ResponseHelper.SendResponse(_callback.LogoutResponse, _responseInfo);
        }
    }
}
