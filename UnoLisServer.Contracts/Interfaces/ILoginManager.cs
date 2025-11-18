using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Common.Models;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(ILoginCallback), SessionMode = SessionMode.Required)]
    public interface ILoginManager
    {
        [OperationContract(IsOneWay = true)]
        void Login(AuthCredentials credentials);
    }

    [ServiceContract]
    public interface ILoginCallback : ISessionCallback
    {
        [OperationContract(IsOneWay = true)]
        void LoginResponse(ServiceResponse<object> response);
    }
}
