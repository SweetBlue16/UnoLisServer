using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Services.Validators
{
    public static class ChatValidator
    {
        public static void ValidateMessage(ChatMessageData data)
        {
            if (data == null)
            {
                throw new ValidationException(MessageCode.EmptyFields, "El objeto de mensaje es nulo.");
            }

            if (string.IsNullOrWhiteSpace(data.Nickname))
            {
                throw new ValidationException(MessageCode.EmptyFields, "El nickname es requerido.");
            }

            if (string.IsNullOrWhiteSpace(data.Message))
            {
                throw new ValidationException(MessageCode.EmptyFields, "No se puede enviar un mensaje vacío.");
            }

            if (data.Message.Length > 255)
            {
                throw new ValidationException(MessageCode.OperationNotSupported, "El mensaje excede los 255 caracteres.");
            }
        }
    }
}