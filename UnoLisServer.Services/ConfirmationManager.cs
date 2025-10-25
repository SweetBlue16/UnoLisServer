using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;

namespace UnoLisServer.Services
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ConfirmationManager : IConfirmationManager
    {
        private readonly UNOContext _context;
        private readonly IConfirmationCallback _callback;
        private ServiceResponse<object> _response;

        private readonly INotificationSender _notificationSender;
        private readonly IVerificationCodeHelper _verificationCodeHelper;
        private readonly IPendingRegistrationHelper _pendingRegistrationHelper;

        public ConfirmationManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<IConfirmationCallback>();

            _notificationSender = NotificationSender.Instance;
            _verificationCodeHelper = VerificationCodeHelper.Instance;
            _pendingRegistrationHelper = PendingRegistrationHelper.Instance;
        }

        public void ConfirmCode(string email, string code)
        {
            Logger.Log($"Intentando confirmar código para '{email}'...");
            try
            {
                var validationRequest = new CodeValidationRequest
                {
                    Identifier = email,
                    Code = code,
                    CodeType = (int)CodeType.EmailVerification,
                    Consume = true
                };
                bool isCodeValid = _verificationCodeHelper.ValidateCode(validationRequest);

                if (!isCodeValid)
                {
                    Logger.Log($"Código inválido o expirado para '{email}'.");
                    _response = new ServiceResponse<object>(false, MessageCode.VerificationCodeInvalid);
                    _callback.ConfirmationResponse(_response);
                    return;
                }

                var pendingData = _pendingRegistrationHelper.GetAndRemovePendingRegistration(email);
                if (pendingData == null)
                {
                    Logger.Log($"Código válido, pero no se encontraron datos de registro para {email}.");
                    _response = new ServiceResponse<object>(false, MessageCode.RegistrationDataLost);
                    _callback.ConfirmationResponse(_response);
                    return;
                }

                using (var transaction = _context.Database.BeginTransaction())
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

                    Logger.Log($"Cuenta {email} verificada y creada exitosamente.");
                    _response = new ServiceResponse<object>(true, MessageCode.RegistrationSuccessful);
                    _callback.ConfirmationResponse(_response);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error en ConfirmCode({email}): {ex.Message}");
                _response = new ServiceResponse<object>(false, MessageCode.GeneralServerError);
                _callback.ConfirmationResponse(_response);
            }
        }

        public void ResendConfirmationCode(string email)
        {
            Logger.Log($"Solicitud de reenvío de código para {email}...");
            try
            {
                if (!_verificationCodeHelper.CanRequestCode(email, (int)CodeType.EmailVerification))
                {
                    _response = new ServiceResponse<object>(false, MessageCode.RateLimitExceeded);
                    Logger.Log($"Demasiadas solicitudes de código para {email}. Rechazando reenvío...");
                    _callback.ResendCodeResponse(_response);
                    return;
                }

                var newCode = _verificationCodeHelper.GenerateAndStoreCode(email, CodeType.EmailVerification);
                _ = _notificationSender.SendAccountVerificationEmailAsync(email, newCode);

                Logger.Log($"Nuevo código enviado a '{email}'.");
                _response = new ServiceResponse<object>(true, MessageCode.VerificationCodeResent);
                _callback.ResendCodeResponse(_response);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error en ResendConfirmationCode({email}): {ex.Message}");
                _response = new ServiceResponse<object>(false, MessageCode.GeneralServerError);
                _callback.ResendCodeResponse(_response);
            }
        }
    }
}
