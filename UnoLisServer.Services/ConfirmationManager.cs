using System;
using System.Data.SqlClient;
using System.ServiceModel;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Services.Validators;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ConfirmationManager : IConfirmationManager
    {
        private readonly UNOContext _context;
        private readonly IConfirmationCallback _callback;
        private ResponseInfo<object> _responseInfo;

        private readonly INotificationSender _notificationSender;
        private readonly IVerificationCodeHelper _verificationCodeHelper;

        public ConfirmationManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<IConfirmationCallback>();

            _notificationSender = NotificationSender.Instance;
            _verificationCodeHelper = VerificationCodeHelper.Instance;
        }

        public void ConfirmCode(string email, string code)
        {
            Logger.Log($"[INFO] Intentando confirmar código para '{email}'...");
            try
            {
                var pendingData = ConfirmationValidator.ValidateConfirmation(email, code);
                CreateAccountFromPending(email, pendingData);

                _responseInfo = new ResponseInfo<object>(
                    MessageCode.RegistrationSuccessful,
                    true,
                    $"[INFO] Cuenta creada exitosamente para '{email}'."
                );
            }
            catch (ValidationException validationEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    validationEx.ErrorCode,
                    false,
                    $"[WARNING] Validación fallida para '{email}'. Error: {validationEx.Message}"
                );
            }
            catch (SqlException dbEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.DatabaseError,
                    false,
                    $"[FATAL] Error de base de datos al confirmar código para '{email}'. Error: {dbEx.Message}"
                );
            }
            catch (CommunicationException communicationEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.ConnectionFailed,
                    false,
                    $"[ERROR] Comunicación al confirmar código para '{email}'. Error: {communicationEx.Message}"
                );
            }
            catch (TimeoutException timeoutEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.Timeout,
                    false,
                    $"[ERROR] Tiempo de espera al confirmar código para '{email}'. Error: {timeoutEx.Message}"
                );
            }
            catch (Exception ex)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.ConfirmationInternalError,
                    false,
                    $"[FATAL] Error inesperado al confirmar código para '{email}'. Error: {ex.Message}"
                );
            }
            ResponseHelper.SendResponse(_callback.ConfirmationResponse, _responseInfo);
        }

        public void ResendConfirmationCode(string email)
        {
            Logger.Log($"[INFO] Solicitud de reenvío de código para {email}...");
            try
            {
                ConfirmationValidator.ValidateResend(email);

                var newCode = _verificationCodeHelper.GenerateAndStoreCode(email, CodeType.EmailVerification);
                _ = _notificationSender.SendAccountVerificationEmailAsync(email, newCode);

                _responseInfo = new ResponseInfo<object>(
                    MessageCode.VerificationCodeResent,
                    true,
                    $"[INFO] Código de verificación reenviado a '{email}'."
                );
            }
            catch (ValidationException validationEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    validationEx.ErrorCode,
                    false,
                    $"[WARNING] Validación fallida para reenvío de código a '{email}'. Error: {validationEx.Message}"
                );
            }
            catch (CommunicationException communicationEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.ConnectionFailed,
                    false,
                    $"[ERROR] Comunicación al reenviar código a '{email}'. Error: {communicationEx.Message}"
                );
            }
            catch (TimeoutException timeoutEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.Timeout,
                    false,
                    $"[ERROR] Tiempo de espera al reenviar código a '{email}'. Error: {timeoutEx.Message}"
                );
            }
            catch (Exception ex)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.ConfirmationInternalError,
                    false,
                    $"[FATAL] Error inesperado al reenviar código a '{email}'. Error: {ex.Message}"
                );
            }
            ResponseHelper.SendResponse(_callback.ResendCodeResponse, _responseInfo);
        }

        public void CreateAccountFromPending(string email, PendingRegistration pendingData)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var newPlayer = new Player
                    {
                        nickname = pendingData.Nickname,
                        fullName = pendingData.FullName
                    };
                    _context.Player.Add(newPlayer);
                    var newAccount = new Account
                    {
                        email = email,
                        password = pendingData.HashedPassword,
                        Player = newPlayer
                    };
                    _context.Account.Add(newAccount);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
