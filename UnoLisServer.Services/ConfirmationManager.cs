using System;
using System.ServiceModel;
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
            string safeEmail = email ?? "Unknown";
            Logger.Log($"[INFO] Confirming code for '{safeEmail}'...");
            ResponseInfo<object> responseInfo = null;

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
                    throw new ValidationException(MessageCode.VerificationCodeInvalid, "Invalid or expired code.");
                }

                var pendingData = _pendingRegistrationHelper.GetAndRemovePendingRegistration(email);
                if (pendingData == null)
                {
                    throw new ValidationException(MessageCode.RegistrationDataLost, "Registration session expired. " +
                        "Please register again.");
                }

                await _playerRepository.CreatePlayerFromPendingAsync(email, pendingData);

                responseInfo = new ResponseInfo<object>(
                    MessageCode.RegistrationSuccessful,
                    true,
                    "Account created successfully."
                );
            }
            catch (ValidationException valEx)
            {
                Logger.Warn($"[CONFIRM] Validation error: {valEx.Message}");
                responseInfo = new ResponseInfo<object>(valEx.ErrorCode, false, valEx.Message);
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Account creation failed. Data Store unavailable.", ex);

                responseInfo = new ResponseInfo<object>(
                    MessageCode.DatabaseError,
                    false,
                    "Service unavailable. Please try again later."
                );
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Account creation timeout.");
                responseInfo = new ResponseInfo<object>(
                    MessageCode.Timeout,
                    false,
                    "Request timed out."
                );
            }
            catch (Exception ex) when (ex.Message == "Data_Conflict")
            {
                Logger.Warn($"[DATA] Constraint violation creating account.");
                responseInfo = new ResponseInfo<object>(
                    MessageCode.RegistrationInternalError,
                    false,
                    "Account creation failed. Email might already be in use."
                );
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error confirming code", ex);
                responseInfo = new ResponseInfo<object>(
                    MessageCode.ConfirmationInternalError,
                    false,
                    "An unexpected error occurred."
                );
            }

            try
            {
                if (_callback != null && responseInfo != null)
                {
                    ResponseHelper.SendResponse(_callback.ConfirmationResponse, responseInfo);
                }
            }
            catch (Exception sendEx)
            {
                Logger.Warn($"[WCF] Failed to send confirmation response. {sendEx}.");
            }
        }

        public async void ResendConfirmationCode(string email)
        {
            string safeEmail = email ?? "Unknown";
            Logger.Log($"[INFO] Resending code to '{safeEmail}'...");
            ResponseInfo<object> responseInfo = null;

            try
            {
                ConfirmationValidator.ValidateResendInput(email);

                if (!_verificationCodeHelper.CanRequestCode(email, CodeType.EmailVerification))
                {
                    throw new ValidationException(MessageCode.RateLimitExceeded, "Please wait before " +
                        "requesting a new code.");
                }

                var newCode = _verificationCodeHelper.GenerateAndStoreCode(email, CodeType.EmailVerification);

                await _notificationSender.SendAccountVerificationEmailAsync(email, newCode);

                responseInfo = new ResponseInfo<object>(
                    MessageCode.VerificationCodeResent,
                    true,
                    "Verification code resent."
                );
            }
            catch (ValidationException valEx)
            {
                responseInfo = new ResponseInfo<object>(valEx.ErrorCode, false, valEx.Message);
            }
            catch (Exception ex)
            {
                Logger.Error($"[ERROR] Failed to resend code to {safeEmail}", ex);
                responseInfo = new ResponseInfo<object>(
                    MessageCode.ConfirmationInternalError,
                    false,
                    "Failed to send email. Please try again later."
                );
            }

            try
            {
                if (_callback != null && responseInfo != null)
                {
                    ResponseHelper.SendResponse(_callback.ResendCodeResponse, responseInfo);
                }
            }
            catch (Exception sendEx)
            {
                Logger.Warn($"[WCF] Failed to send resend-code response, {sendEx}.");
            }
        }
    }
}