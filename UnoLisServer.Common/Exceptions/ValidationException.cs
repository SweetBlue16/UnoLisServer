using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;

namespace UnoLisServer.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public MessageCode ErrorCode { get; private set; }

        public ValidationException(MessageCode errorCode, string message = null) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
