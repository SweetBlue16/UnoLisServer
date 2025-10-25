using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class RegisterManager : IRegisterManager
    {
        private readonly UNOContext _context;
        private readonly IRegisterCallback _callback;
        private ServiceResponse<object> _response;

        private readonly INotificationSender _notificationSender;
        private readonly IVerificationCodeHelper _verificationCodeHelper;
        private readonly IPendingRegistrationHelper _pendingRegistrationHelper;

        public RegisterManager()
        {
            _context = new UNOContext();
            _callback = OperationContext.Current.GetCallbackChannel<IRegisterCallback>();

            _notificationSender = NotificationSender.Instance;
            _verificationCodeHelper = VerificationCodeHelper.Instance;
            _pendingRegistrationHelper = PendingRegistrationHelper.Instance;
        }

        public void Register(RegistrationData data)
        {
            if (data == null)
            {
                _response = new ServiceResponse<object>(false, MessageCode.InvalidData);
                _callback.RegisterResponse(_response);
                return;
            }

            try
            {
                Logger.Log($"Intentando registrar el usuario '{data.Nickname}'...");

                bool existsPlayer = _context.Player.Any(p => p.nickname == data.Nickname);
                if (existsPlayer)
                {
                    _response = new ServiceResponse<object>(false, MessageCode.NicknameAlreadyTaken);
                    _callback.RegisterResponse(_response);
                    Logger.Log($"El nickname '{data.Nickname}' ya está registrado.");
                    return;
                }

                bool existsAccount = _context.Account.Any(a => a.email == data.Email);
                if (existsAccount)
                {
                    _response = new ServiceResponse<object>(false, MessageCode.EmailAlreadyRegistered);
                    _callback.RegisterResponse(_response);
                    Logger.Log($"El email '{data.Email}' ya está registrado.");
                    return;
                }

                var pendingData = new PendingRegistration
                {
                    Nickname = data.Nickname,
                    FullName = data.FullName,
                    HashedPassword = PasswordHelper.HashPassword(data.Password)
                };

                _pendingRegistrationHelper.StorePendingRegistration(data.Email, pendingData);
                var code = _verificationCodeHelper.GenerateAndStoreCode(data.Email, CodeType.EmailVerification);
                _notificationSender.SendAccountVerificationEmailAsync(data.Email, code);

                _response = new ServiceResponse<object>(true, MessageCode.VerificationCodeSent);
                _callback.RegisterResponse(_response);
                Logger.Log($"Código enviado a {data.Email}. Esperando confirmación...");
            }
            catch (CommunicationException communicationEx)
            {
                Logger.Log($"Error de comunicación en Register({data.Email}): {communicationEx.Message}");
                _response = new ServiceResponse<object>(false, MessageCode.ConnectionFailed);
                _callback.RegisterResponse(_response);
            }
            catch (TimeoutException timeoutEx)
            {
                Logger.Log($"Timeout en Register({data.Email}): {timeoutEx.Message}");
                _response = new ServiceResponse<object>(false, MessageCode.Timeout);
                _callback.RegisterResponse(_response);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error en Register({data.Email}): {ex.Message}");
                _response = new ServiceResponse<object>(false, MessageCode.GeneralServerError);
                _callback.RegisterResponse(_response);
            }
        }
    }
}
