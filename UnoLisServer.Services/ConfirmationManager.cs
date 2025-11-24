using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Data.SqlClient;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services.Validators;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ConfirmationManager : IConfirmationManager
    {
        private readonly IConfirmationCallback _callback;
        private readonly IPlayerRepository _playerRepository;

        private readonly INotificationSender _notificationSender;
        private readonly IVerificationCodeHelper _verificationCodeHelper;
        private readonly IPendingRegistrationHelper _pendingRegistrationHelper;

        public ConfirmationManager() : this(new PlayerRepository())
        {
        }

        public ConfirmationManager(IPlayerRepository repository,
                                   IConfirmationCallback callback = null,
                                   INotificationSender notificationSender = null,
                                   IVerificationCodeHelper verificationHelper = null,
                                   IPendingRegistrationHelper pendingHelper = null)
        {
            _playerRepository = repository;
            _callback = callback ?? OperationContext.Current?.GetCallbackChannel<IConfirmationCallback>();

            _notificationSender = notificationSender ?? NotificationSender.Instance;
            _verificationCodeHelper = verificationHelper ?? VerificationCodeHelper.Instance;
            _pendingRegistrationHelper = pendingHelper ?? PendingRegistrationHelper.Instance;
        }

        public async void ConfirmCode(string email, string code)
        {
            Logger.Log($"[INFO] Confirmando código para '{email}'...");
            ResponseInfo<object> responseInfo;

            try
            {
                ConfirmationValidator.ValidateInput(email, code);

                var request = new CodeValidationRequest
                {
                    Identifier = email,
                    Code = code,
                    CodeType = (int)CodeType.EmailVerification,
                    Consume = true
                };

                if (!_verificationCodeHelper.ValidateCode(request))
                {
                    throw new ValidationException(MessageCode.VerificationCodeInvalid, "Código inválido o expirado.");
                }

                var pendingData = _pendingRegistrationHelper.GetAndRemovePendingRegistration(email);
                if (pendingData == null)
                {
                    throw new ValidationException(MessageCode.RegistrationDataLost, "Datos de registro no encontrados. Regístrese de nuevo.");
                }

                await _playerRepository.CreatePlayerFromPendingAsync(email, pendingData);

                responseInfo = new ResponseInfo<object>(
                    MessageCode.RegistrationSuccessful,
                    true,
                    $"[INFO] Cuenta creada para '{email}'."
                );
            }
            catch (ValidationException valEx)
            {
                responseInfo = new ResponseInfo<object>(valEx.ErrorCode, false, valEx.Message);
                Logger.Warn($"[CONFIRM] Error validación: {valEx.Message}");
            }
            catch (Exception ex)
            {
                responseInfo = new ResponseInfo<object>(MessageCode.ConfirmationInternalError, false, "Error al confirmar cuenta.");
                Logger.Error($"[ERROR] Confirmación fallida para {email}", ex);
            }

            if (_callback != null)
            {
                ResponseHelper.SendResponse(_callback.ConfirmationResponse, responseInfo);
            }
        }

        public async void ResendConfirmationCode(string email)
        {
            Logger.Log($"[INFO] Reenviando código a '{email}'...");
            ResponseInfo<object> responseInfo;

            try
            {
                ConfirmationValidator.ValidateResendInput(email);

                if (!_verificationCodeHelper.CanRequestCode(email, CodeType.EmailVerification))
                {
                    throw new ValidationException(MessageCode.RateLimitExceeded, "Espere antes de pedir otro código.");
                }

                var newCode = _verificationCodeHelper.GenerateAndStoreCode(email, CodeType.EmailVerification);

                await _notificationSender.SendAccountVerificationEmailAsync(email, newCode);

                responseInfo = new ResponseInfo<object>(
                    MessageCode.VerificationCodeResent,
                    true,
                    "Código reenviado."
                );
            }
            catch (ValidationException valEx)
            {
                responseInfo = new ResponseInfo<object>(valEx.ErrorCode, false, valEx.Message);
            }
            catch (Exception ex)
            {
                responseInfo = new ResponseInfo<object>(MessageCode.ConfirmationInternalError, false, "Error al reenviar código.");
                Logger.Error($"[ERROR] Reenvío fallido para {email}", ex);
            }

            if (_callback != null)
            {
                ResponseHelper.SendResponse(_callback.ResendCodeResponse, responseInfo);
            }
        }
    }
}