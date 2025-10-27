using System;
using UnoLisServer.Common.Enums;

namespace UnoLisServer.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public MessageCode ErrorCode { get; }

        public ValidationException(MessageCode errorCode, string message = null) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
