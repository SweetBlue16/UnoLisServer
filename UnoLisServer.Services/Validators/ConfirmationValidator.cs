using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;

namespace UnoLisServer.Services.Validators
{
    public static class ConfirmationValidator
    {
        private static readonly IVerificationCodeHelper _verificationCodeHelper =
            VerificationCodeHelper.Instance;
        private static readonly IPendingRegistrationHelper _pendingRegistrationHelper =
            PendingRegistrationHelper.Instance;

        public static PendingRegistration ValidateConfirmation(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            {
                throw new ValidationException(MessageCode.InvalidData,
                    "El email y el código no pueden estar vacíos.");
            }

            var validationRequest = new CodeValidationRequest
            {
                Identifier = email,
                Code = code,
                CodeType = (int) CodeType.EmailVerification,
                Consume = true
            };

            bool isCodeValid = _verificationCodeHelper.ValidateCode(validationRequest);
            if (!isCodeValid)
            {
                throw new ValidationException(MessageCode.VerificationCodeInvalid,
                    $"El código de verificación inválido o expirado para '{email}'.");
            }

            var pendingData = _pendingRegistrationHelper.GetAndRemovePendingRegistration(email);
            if (pendingData == null)
            {
                throw new ValidationException(MessageCode.RegistrationDataLost,
                    $"Código válido, pero no se encontraron datos de registro para '{email}'.");
            }

            return pendingData;
        }

        public static void ValidateResend(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ValidationException(MessageCode.InvalidData,
                    "El email no puede estar vacío.");
            }

            if (!_verificationCodeHelper.CanRequestCode(email, (int) CodeType.EmailVerification))
            {
                throw new ValidationException(MessageCode.RateLimitExceeded,
                    $"Demasiadas solicitudes de código para '{email}'.");
            }
        }
    }
}
