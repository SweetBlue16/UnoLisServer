using System;
using System.ServiceModel;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services.Validators;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class RegisterManager : IRegisterManager
    {
        private readonly IRegisterCallback _callback;
        private readonly IPlayerRepository _playerRepository;

        private readonly INotificationSender _notificationSender;
        private readonly IVerificationCodeHelper _verificationCodeHelper;
        private readonly IPendingRegistrationHelper _pendingRegistrationHelper;

        public RegisterManager() : this(new PlayerRepository())
        {
        }

        public RegisterManager(IPlayerRepository playerRepository,
                               IRegisterCallback callbackTest = null,
                               INotificationSender notificationSender = null,
                               IVerificationCodeHelper verificationHelper = null,
                               IPendingRegistrationHelper pendingHelper = null)
        {
            _playerRepository = playerRepository;
            _callback = callbackTest ?? OperationContext.Current?.GetCallbackChannel<IRegisterCallback>();

            _notificationSender = notificationSender ?? NotificationSender.Instance;
            _verificationCodeHelper = verificationHelper ?? VerificationCodeHelper.Instance;
            _pendingRegistrationHelper = pendingHelper ?? PendingRegistrationHelper.Instance;
        }

        public async void Register(RegistrationData data)
        {
            string email = data?.Email ?? "Unknown";
            string nickname = data?.Nickname ?? "Unknown";
            ResponseInfo<object> responseInfo;

            try
            {
                RegisterValidator.ValidateFormats(data);

                if (await _playerRepository.IsNicknameTakenAsync(data.Nickname))
                {
                    throw new ValidationException(MessageCode.NicknameAlreadyTaken, $"Nickname is already taken.");
                }

                if (await _playerRepository.IsEmailRegisteredAsync(data.Email))
                {
                    throw new ValidationException(MessageCode.EmailAlreadyRegistered, $"Email is already registered.");
                }

                if (!_verificationCodeHelper.CanRequestCode(data.Email, CodeType.EmailVerification))
                {
                    throw new ValidationException(MessageCode.RateLimitExceeded, "Too many requests. Please wait");
                }

                var pendingData = new PendingRegistration
                {
                    Nickname = data.Nickname,
                    FullName = data.FullName,
                    HashedPassword = PasswordHelper.HashPassword(data.Password)
                };

                _pendingRegistrationHelper.StorePendingRegistration(data.Email, pendingData);
                var code = _verificationCodeHelper.GenerateAndStoreCode(data.Email, CodeType.EmailVerification);
                await _notificationSender.SendAccountVerificationEmailAsync(data.Email, code);

                responseInfo = new ResponseInfo<object>(
                    MessageCode.VerificationCodeSent,
                    true,
                    "Verification code sent successfully."
                );
            }
            catch (ValidationException valEx)
            {
                Logger.Warn($"[REGISTER] Validation failed for {email}: {valEx.Message}");
                responseInfo = new ResponseInfo<object>(valEx.ErrorCode, false, valEx.Message);
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Register failed for {email}. Database/Network unavailable.", ex);
                responseInfo = new ResponseInfo<object>(
                    MessageCode.DatabaseError,
                    false,
                    "Service unavailable. Please try again later."
                );
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Register failed for {email}. Server busy/timeout.");
                responseInfo = new ResponseInfo<object>(
                    MessageCode.Timeout,
                    false,
                    "Server request timed out."
                );
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error during register for {email}", ex);
                responseInfo = new ResponseInfo<object>(
                    MessageCode.RegistrationInternalError,
                    false,
                    "An unexpected error occurred during registration."
                );
            }

            try
            {
                if (_callback != null && responseInfo != null)
                {
                    ResponseHelper.SendResponse(_callback.RegisterResponse, responseInfo);
                }
            }
            catch (Exception sendEx)
            {
                Logger.Warn($"[WCF] Failed to send register response. Client might have disconnected '{sendEx}'.");
            }
        }
    }
}