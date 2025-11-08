using System;
using System.ServiceModel;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Services.Validators;
using UnoLisServer.Common.Exceptions;
using System.Data.SqlClient;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class RegisterManager : IRegisterManager
    {
        private readonly IRegisterCallback _callback;
        private ResponseInfo<object> _responseInfo;

        private readonly INotificationSender _notificationSender;
        private readonly IVerificationCodeHelper _verificationCodeHelper;
        private readonly IPendingRegistrationHelper _pendingRegistrationHelper;

        public RegisterManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<IRegisterCallback>();

            _notificationSender = NotificationSender.Instance;
            _verificationCodeHelper = VerificationCodeHelper.Instance;
            _pendingRegistrationHelper = PendingRegistrationHelper.Instance;
        }

        public void Register(RegistrationData data)
        {
            string email = data?.Email ?? "Unknown";
            string nickname = data?.Nickname ?? "Unknown";
            try
            {
                RegisterValidator.ValidateRegistrationData(data);
                Logger.Log($"[INFO] Intentando registrar el usuario '{nickname}' con el correo '{email}'...");
                RegisterValidator.CheckExistingUser(data);

                var pendingData = new PendingRegistration
                {
                    Nickname = data.Nickname,
                    FullName = data.FullName,
                    HashedPassword = PasswordHelper.HashPassword(data.Password)
                };

                RegisterValidator.CanRequestVerificationCode(data.Email);

                _pendingRegistrationHelper.StorePendingRegistration(data.Email, pendingData);
                var code = _verificationCodeHelper.GenerateAndStoreCode(data.Email, CodeType.EmailVerification);
                _notificationSender.SendAccountVerificationEmailAsync(data.Email, code);

                _responseInfo = new ResponseInfo<object>(
                    MessageCode.VerificationCodeSent,
                    true,
                    $"[INFO] Código de verificación enviado a '{email}' para el usuario '{nickname}'. Esperando confirmación..."
                );
            }
            catch (ValidationException validationEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    validationEx.ErrorCode,
                    false,
                    $"[WARNING] Validación durante el registro de '{nickname}' con el correo '{email}': {validationEx.Message}"
                );
            }
            catch (SqlException dbEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.DatabaseError,
                    false,
                    $"[ERROR] Error de base de datos durante el registro de '{nickname}' con el correo '{email}': {dbEx.Message}"
                );
            }
            catch (CommunicationException communicationEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.ConnectionFailed,
                    false,
                    $"[ERROR] Comunicación durante el registro de '{nickname}' con el correo '{email}'. Error: {communicationEx.Message}"
                );
            }
            catch (TimeoutException timeoutEx)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.Timeout,
                    false,
                    $"[ERROR] Tiempo de espera agotado durante el registro de '{nickname}' con el correo '{email}'. Error: {timeoutEx.Message}"
                );
            }
            catch (Exception ex)
            {
                _responseInfo = new ResponseInfo<object>(
                    MessageCode.RegistrationInternalError,
                    false,
                    $"[ERROR] Error general durante el registro de '{nickname}' con el correo '{email}': {ex.Message}"
                );
            }
            ResponseHelper.SendResponse(_callback.RegisterResponse, _responseInfo);
        }
    }
}
