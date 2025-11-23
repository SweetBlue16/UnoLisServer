using UnoLisServer.Common.Enums;

namespace UnoLisServer.Common.Models
{
    public class ResponseInfo<T>
    {
        public MessageCode MessageCode { get; set; }
        public bool Success { get; set; }
        public string LogMessage { get; set; }
        public T Data { get; set; }

        public ResponseInfo(MessageCode messageCode, bool success = false, string logMessage = "", T data = default(T))
        {
            Success = success;
            MessageCode = messageCode;
            LogMessage = logMessage;
            Data = data;
        }
    }
}
