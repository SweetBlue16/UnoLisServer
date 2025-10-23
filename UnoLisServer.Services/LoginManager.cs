using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Common.Enums;
using System.Data.SqlClient;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LoginManager : ILoginManager
    {
        private readonly UNOContext _context;
        private readonly ILoginCallback _callback;
        private ServiceResponse<object> _response;

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
                    _response = new ServiceResponse<object>(false, MessageCode.EmptyFields);
                    _callback.LoginResponse(_response);
                    return;
                }
                Logger.Log($"Intentando inicio de sesión para '{credentials.Nickname}'...");

                var account = _context.Account.FirstOrDefault(a => a.Player.nickname == credentials.Nickname);
                if (account == null)
                {
                    _response = new ServiceResponse<object>(false, MessageCode.PlayerNotFound);
                    _callback.LoginResponse(_response);
                    Logger.Log($"Usuario '{credentials.Nickname}' no encontrado.");
                    return;
                }

                bool isPasswordValid = PasswordHelper.VerifyPassword(credentials.Password, account.password);
                if (!isPasswordValid)
                {
                    _response = new ServiceResponse<object>(false, MessageCode.InvalidCredentials);
                    _callback.LoginResponse(_response);
                    Logger.Log($"Contraseña incorrecta para '{credentials.Nickname}'.");
                    return;
                }

                if (SessionManager.IsOnline(account.Player.nickname))
                {
                    _response = new ServiceResponse<object>(false, MessageCode.DuplicateSession);
                    _callback.LoginResponse(_response);
                    Logger.Log($"Sesión duplicada detectada para '{credentials.Nickname}'");
                    return;
                }

                var session = OperationContext.Current.GetCallbackChannel<ILoginCallback>();
                SessionManager.AddSession(account.Player.nickname, session);

                _response = new ServiceResponse<object>(true, MessageCode.LoginSuccessful);
                _callback.LoginResponse(_response);
                Logger.Log($"Usuario '{account.Player.nickname}' inició sesión correctamente.");
            }
            catch (CommunicationException communicationEx)
            {
                _response = new ServiceResponse<object>(false, MessageCode.ConnectionFailed);
                Logger.Log($"Error de comunicación con '{credentials?.Nickname ?? "desconocido"}'. Error: {communicationEx.Message}");
                _callback.LoginResponse(_response);
            }
            catch (TimeoutException timeoutEx)
            {
                _response = new ServiceResponse<object>(false, MessageCode.Timeout);
                Logger.Log($"Tiempo de espera agotado para '{credentials?.Nickname ?? "desconocido"}'. Error: {timeoutEx.Message}");
                _callback.LoginResponse(_response);
            }
            catch (SqlException dbEx)
            {
                _response = new ServiceResponse<object>(false, MessageCode.DatabaseError);
                Logger.Log($"Error de base de datos durante el inicio de sesión de '{credentials?.Nickname ?? "null"}': {dbEx}");
                _callback.LoginResponse(_response);
            }
            catch (Exception ex)
            {
                _response = new ServiceResponse<object>(false, MessageCode.LoginInternalError);
                Logger.Log($"Fallo en el Login('{credentials?.Nickname ?? "null"}'): {ex}");
                _callback.LoginResponse(_response);
            }
        }
    }
}
