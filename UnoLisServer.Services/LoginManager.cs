using System;
using System.ServiceModel;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Common.Enums;
using System.Data.SqlClient;
using UnoLisServer.Services.Validators;
using UnoLisServer.Common.Exceptions;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LoginManager : ILoginManager
    {
        private readonly ILoginCallback _callback;
        private ResponseInfo<object> _responseInfo;

        public LoginManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<ILoginCallback>();
        }

        public void Login(AuthCredentials credentials)
        {
            string nickname = credentials?.Nickname ?? "Unknown";
            try
            {
                LoginValidator.ValidateCredentials(credentials);
                Logger.Log($"[INFO] Intentando inicio de sesión para '{nickname}'...");
                LoginValidator.AuthenticatePlayer(credentials);

                var banInfo = LoginValidator.IsPlayerBanned(nickname);
                if (banInfo != null)
                {
                    var banResponse = CreateBanResponse(banInfo);
                    ResponseHelper.SendResponse(_callback.LoginResponse, banResponse);
                    return;
                }

                var session = OperationContext.Current.GetCallbackChannel<ILoginCallback>();
                SessionManager.AddSession(nickname, session);

                _responseInfo = new ResponseInfo<object>(
                    MessageCode.LoginSuccessful,
                    true,
                    $"[INFO] Usuario {nickname} inició sesión correctamente."
                );
            }
            catch (ValidationException validationEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    validationEx.ErrorCode,
                    false,
                    $"[WARNING] Validación durante el inicio de sesión de '{nickname}': {validationEx.Message}"
                );
            }
            catch (CommunicationException communicationEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.ConnectionFailed,
                    false,
                    $"[ERROR] Comunicación con '{nickname}'. Error: {communicationEx.Message}"
                );
            }
            catch (TimeoutException timeoutEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.Timeout,
                    false,
                    $"[ERROR] Tiempo de espera agotado para '{nickname}'. Error: {timeoutEx.Message}"
                );
            }
            catch (SqlException dbEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.DatabaseError,
                    false,
                    $"[ERROR] Base de datos durante el inicio de sesión de '{nickname}': {dbEx.Message}"
                );
            }
            catch (Exception ex)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.LoginInternalError,
                    false,
                    $"[ERROR] Excepción no controlada durante el inicio de sesión de '{nickname}': {ex.Message}"
                );
            }
            ResponseHelper.SendResponse(_callback.LoginResponse, _responseInfo);
        }

        private ResponseInfo<object> CreateBanResponse(BanInfo banInfo)
        {
            ResponseInfo<object> responseInfo = null;
            if (banInfo != null)
            {
                responseInfo = new ResponseInfo<object>(
                    MessageCode.PlayerBanned,
                    false,
                    $"[WARNING] Intento de inicio de sesión de usuario baneado. Tiempo restante: {banInfo.FormattedTimeRemaining}",
                    banInfo
                );
            }
            return responseInfo;
        }
    }
}
