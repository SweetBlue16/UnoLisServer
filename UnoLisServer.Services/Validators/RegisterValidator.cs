using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Services.Validators
{
    public static class RegisterValidator
    {
        public static void ValidateFormats(RegistrationData data)
        {
            if (data == null ||
                string.IsNullOrWhiteSpace(data.Email) ||
                string.IsNullOrWhiteSpace(data.Password) ||
                string.IsNullOrWhiteSpace(data.Nickname) ||
                string.IsNullOrWhiteSpace(data.FullName))
            {
                throw new ValidationException(MessageCode.EmptyFields, "Todos los campos son obligatorios.");
            }

            // Aquí podrías agregar ValidateEmail(data.Email) si quieres reusar la lógica de ProfileEdit
        }
    }
}