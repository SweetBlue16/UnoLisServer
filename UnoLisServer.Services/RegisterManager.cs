using System;
using System.ServiceModel;
using System.Threading.Tasks;
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
                    throw new ValidationException(MessageCode.NicknameAlreadyTaken, $"El nickname '{data.Nickname}' ya está en uso.");
                }

                if (await _playerRepository.IsEmailRegisteredAsync(data.Email))
                {
                    throw new ValidationException(MessageCode.EmailAlreadyRegistered, $"El email '{data.Email}' ya está registrado.");
                }

                if (!_verificationCodeHelper.CanRequestCode(data.Email, CodeType.EmailVerification))
                {
                    throw new ValidationException(MessageCode.RateLimitExceeded, "Demasiadas solicitudes. Intente más tarde.");
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
                    $"[INFO] Código enviado a '{email}'."
                );
            }
            catch (ValidationException valEx)
            {
                responseInfo = new ResponseInfo<object>(valEx.ErrorCode, false, valEx.Message);
                Logger.Warn($"[REGISTER] Validación fallida: {valEx.Message}");
            }
            catch (Exception ex)
            {
                responseInfo = new ResponseInfo<object>(MessageCode.RegistrationInternalError, false, "Error interno en el registro.");
                Logger.Error($"[ERROR] Fallo en registro para {email}", ex);
            }

            if (_callback != null)
            {
                ResponseHelper.SendResponse(_callback.RegisterResponse, responseInfo);
            }
        }
    }
}