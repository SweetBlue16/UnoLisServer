using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            callback?.Invoke(response);

            if (!string.IsNullOrWhiteSpace(info.LogMessage))
            {
                Logger.Log(info.LogMessage);
            }
        }
    }
}
