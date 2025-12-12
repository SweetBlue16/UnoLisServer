namespace UnoLisServer.Common.Models
{
    /// <summary>
    /// Class representing the result of an operation with success status and message.
    /// </summary>
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public static OperationResult Ok(string msg = "Successful operation") =>
            new OperationResult { Success = true, Message = msg };

        public static OperationResult Fail(string msg = "Error in the operation") =>
            new OperationResult { Success = false, Message = msg };
    }
}
