using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;

namespace UnoLisServer.Common.Models
{
    [DataContract]
    public class ServiceResponse<T>
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public MessageCode Code { get; set; }

        [DataMember]
        public T Data { get; set; }

        public ServiceResponse() { }

        public ServiceResponse(bool success, MessageCode code, T data = default)
        {
            Success = success;
            Code = code;
            Data = data;
        }
    }
}
