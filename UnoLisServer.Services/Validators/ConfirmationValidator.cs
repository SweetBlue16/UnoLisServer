using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;

namespace UnoLisServer.Services.Validators
{
    public static class ConfirmationValidator
    {
        public static void ValidateInput(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            {
                throw new ValidationException(MessageCode.EmptyFields, "El email y el código son obligatorios.");
            }
        }

        public static void ValidateResendInput(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ValidationException(MessageCode.EmptyFields, "El email no puede estar vacío.");
            }
        }
    }
}