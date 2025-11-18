using System;
using UnoLisServer.Common.Models;


namespace UnoLisServer.Common.Helpers
{
    public static class ResponseHelper
    {
        public static void SendResponse<T>(Action<ServiceResponse<T>> callback, ResponseInfo<T> info)
        {
            var response = new ServiceResponse<T>()
            {
                Success = info.Success,
                Code = info.MessageCode,
                Data = info.Data
            };

            try
            {
                callback?.Invoke(response);
            }
            catch (Exception ex)
            {
                Logger.Log($"[UNEXPECTED ERROR] {ex.GetType().Name}: {ex.Message}");
                Logger.Log(ex.StackTrace);
            }

            if (!string.IsNullOrWhiteSpace(info.LogMessage))
            {
                Logger.Log(info.LogMessage);
            }
        }

    }
}
