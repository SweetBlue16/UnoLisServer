using System;
using System.ServiceModel;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Common.Enums;
using UnoLisServer.Services.Validators;
using UnoLisServer.Common.Exceptions;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LoginManager : ILoginManager
    {
        private readonly ILoginCallback _callback;

        public LoginManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<ILoginCallback>();
        }

        public void Login(AuthCredentials credentials)
        {
            string nickname = credentials?.Nickname ?? "Unknown";
            ResponseInfo<object> response = null;

            try
            {
                Logger.Log($"[INFO] Attempting login for...");

                LoginValidator.ValidateCredentials(credentials);

                LoginValidator.AuthenticatePlayer(credentials);

                var banInfo = LoginValidator.IsPlayerBanned(nickname);
                if (banInfo != null)
                {
                    Logger.Log($"[AUTH] Login denied for banned user.");
                    response = CreateBanResponse(banInfo);
                    ResponseHelper.SendResponse(_callback.LoginResponse, response);
                    return;
                }

                var sessionCallback = OperationContext.Current.GetCallbackChannel<ILoginCallback>();
                SessionManager.AddSession(nickname, sessionCallback);

                Logger.Log($"[INFO] User logged in successfully.");

                response = new ResponseInfo<object>(
                    MessageCode.LoginSuccessful,
                    true,
                    "Login successful."
                );
            }
            catch (ValidationException validationEx)
            {
                Logger.Warn($"[AUTH] Validation failed: {validationEx.Message}");
                response = new ResponseInfo<object>(
                    validationEx.ErrorCode,
                    false,
                    validationEx.Message 
                );
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[WCF] Communication error with client '{commEx}'.");
                response = new ResponseInfo<object>(MessageCode.ConnectionFailed, false, "Connection error.");
            }
            catch (TimeoutException timeoutEx)
            {
                Logger.Warn($"[WCF] WCF Timeout for '{timeoutEx}'.");
                response = new ResponseInfo<object>(MessageCode.Timeout, false, "Request timed out.");
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Login failed. Data Store unavailable.", ex);
                response = new ResponseInfo<object>(
                    MessageCode.DatabaseError,
                    false,
                    "Service is currently unavailable. Please try again later."
                );
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Login failed. Server busy/timeout.");
                response = new ResponseInfo<object>(
                    MessageCode.Timeout,
                    false,
                    "Server is taking too long to respond."
                );
            }
            catch (Exception ex)
            {
                Logger.Error($"[FATAL] Unhandled exception during login.", ex);

                response = new ResponseInfo<object>(
                    MessageCode.LoginInternalError,
                    false,
                    "An unexpected internal error occurred."
                );
            }

            try
            {
                if (response != null)
                {
                    ResponseHelper.SendResponse(_callback.LoginResponse, response);
                }
            }
            catch (Exception sendEx)
            {
                Logger.Error($"[WCF-FATAL] Failed to send login response.", sendEx);
            }
        }

        private ResponseInfo<object> CreateBanResponse(BanInfo banInfo)
        {
            if (banInfo == null)
            {
                return null;
            }

            return new ResponseInfo<object>(
                MessageCode.PlayerBanned,
                false,
                $"Access denied. Account banned until: {banInfo.FormattedTimeRemaining}",
                banInfo
            );
        }
    }
}